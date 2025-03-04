#include "Optimizer.hpp"
#include "g2o/core/block_solver.h"
#include "g2o/core/optimization_algorithm_levenberg.h"
#include "g2o/solvers/eigen/linear_solver_eigen.h"
#include "g2o/types/sba/types_six_dof_expmap.h"
#include "g2o/core/robust_kernel_impl.h"
#include "g2o/solvers/dense/linear_solver_dense.h"
#include "g2o/types/sim3/types_seven_dof_expmap.h"
#include "Eigen/StdVector"

int Optimizer::PoseOptimization(std::shared_ptr<Frame> frame, const CameraInfo& camInfo)
{
    // Initialize g2o optimizer
    g2o::SparseOptimizer optimizer;

    auto linearSolver = std::make_unique<g2o::LinearSolverDense<g2o::BlockSolver_6_3::PoseMatrixType>>();

    auto solver = new g2o::OptimizationAlgorithmLevenberg(std::make_unique<g2o::BlockSolver_6_3>(std::move(linearSolver)));

    optimizer.setAlgorithm(solver);

    int nInitialCorrespondences = 0;

    // Set the Frame vertex (camera pose)
    g2o::VertexSE3Expmap* vSE3 = new g2o::VertexSE3Expmap();
    cv::Mat Tcw = frame->Tcw();
    Eigen::Matrix3d R;
    R << Tcw.at<double>(0, 0), Tcw.at<double>(0, 1), Tcw.at<double>(0, 2),
        Tcw.at<double>(1, 0), Tcw.at<double>(1, 1), Tcw.at<double>(1, 2),
        Tcw.at<double>(2, 0), Tcw.at<double>(2, 1), Tcw.at<double>(2, 2);
    Eigen::Vector3d t(Tcw.at<double>(0, 3), Tcw.at<double>(1, 3), Tcw.at<double>(2, 3));
    vSE3->setEstimate(g2o::SE3Quat(R, t));
    vSE3->setId(0);
    vSE3->setFixed(false);
    optimizer.addVertex(vSE3);

    const int N = frame->KeyPoints().size();

    std::vector<g2o::EdgeSE3ProjectXYZOnlyPose*> vpEdgesMono;
    std::vector<size_t> vnIndexEdgeMono;
    vpEdgesMono.reserve(N);
    vnIndexEdgeMono.reserve(N);

    const float deltaMono = sqrt(5.991);

    for (int i = 0; i < N; i++)
    {
        MapPoint* p3D = frame->GetMapPoint()[i];
        const cv::KeyPoint& kp = frame->KeyPoints()[i];

        if (p3D) // Check if map point is valid
        {
            frame->Outliers()[i] = false;

            nInitialCorrespondences++;

            Eigen::Matrix<double, 2, 1> obs;
            obs << kp.pt.x, kp.pt.y;

            g2o::EdgeSE3ProjectXYZOnlyPose* e = new g2o::EdgeSE3ProjectXYZOnlyPose();
            e->setVertex(0, dynamic_cast<g2o::OptimizableGraph::Vertex*>(optimizer.vertex(0)));
            e->setMeasurement(obs);
            e->setInformation(Eigen::Matrix2d::Identity());

            e->fx = camInfo.fx;
            e->fy = camInfo.fy;
            e->cx = camInfo.cx;
            e->cy = camInfo.cy;
            e->Xw = Eigen::Vector3d(p3D->getPosition().x, p3D->getPosition().y, p3D->getPosition().z);

            g2o::RobustKernelHuber* rk = new g2o::RobustKernelHuber;
            e->setRobustKernel(rk);
            rk->setDelta(deltaMono);

            optimizer.addEdge(e);

            vpEdgesMono.push_back(e);
            vnIndexEdgeMono.push_back(i);
        }
    }

    if (nInitialCorrespondences < 3)
        return 0;

    // Optimize the pose using multiple iterations, handling outliers
    const float chi2Mono[4] = { 5.991, 5.991, 5.991, 5.991 };
    const int its[4] = { 10, 10, 10, 10 };

    int nBad = 0;
    for (size_t it = 0; it < 4; it++)
    {
        vSE3->setEstimate(g2o::SE3Quat(R, t));
        optimizer.initializeOptimization(0);
        optimizer.optimize(its[it]);

        nBad = 0;
        for (size_t i = 0, iend = vpEdgesMono.size(); i < iend; i++)
        {
            g2o::EdgeSE3ProjectXYZOnlyPose* e = vpEdgesMono[i];
            const size_t idx = vnIndexEdgeMono[i];

            if (frame->Outliers()[i]) // Check if it's not an outlier
            {
                e->computeError();
            }

            const float chi2 = e->chi2();

            if (chi2 > chi2Mono[it])
            {
                frame->Outliers()[i] = true; // Mark as outlier
                e->setLevel(1);
                nBad++;
            }
            else
            {
                e->setLevel(0);
                frame->Outliers()[i] = false; // Mark as inlier
            }

            if (it == 2)
                e->setRobustKernel(0);
        }

        if (optimizer.edges().size() < 10)
            break;
    }

    // Recover optimized pose and return number of inliers
    g2o::VertexSE3Expmap* vSE3_recov = static_cast<g2o::VertexSE3Expmap*>(optimizer.vertex(0));
    g2o::SE3Quat SE3quat_recov = vSE3_recov->estimate();
    Eigen::Matrix4d optimizedPose = SE3quat_recov.to_homogeneous_matrix();

    cv::Mat optimizedPoseCV(4, 4, CV_64F);
    for (int i = 0; i < 4; ++i)
        for (int j = 0; j < 4; ++j)
            optimizedPoseCV.at<double>(i, j) = optimizedPose(i, j);

    frame->SetTcw(optimizedPoseCV);

    return nInitialCorrespondences - nBad;
}
