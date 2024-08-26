#ifndef G2O_CONFIG_H
#define G2O_CONFIG_H

#define G2O_HAVE_OPENGL 1
#define G2O_OPENGL_FOUND 1
/* #undef G2O_OPENMP */
#define G2O_SHARED_LIBS 1
/* #undef G2O_LGPL_SHARED_LIBS */

// available sparse matrix libraries
/* #undef G2O_HAVE_CHOLMOD */
/* #undef G2O_HAVE_CSPARSE */

// logging framework available
/* #undef G2O_HAVE_LOGGING */

/* #undef G2O_NO_IMPLICIT_OWNERSHIP_OF_OBJECTS */

#ifdef G2O_NO_IMPLICIT_OWNERSHIP_OF_OBJECTS
#define G2O_DELETE_IMPLICITLY_OWNED_OBJECTS 0
#else
#define G2O_DELETE_IMPLICITLY_OWNED_OBJECTS 1
#endif

#define G2O_CXX_COMPILER "MSVC D:/Program Files/Microsoft Visual Studio/2022/Community/VC/Tools/MSVC/14.40.33807/bin/Hostx64/x64/cl.exe"
#define G2O_SRC_DIR "D:/work/125.MyRobotics/stuff/g2o"

#endif
