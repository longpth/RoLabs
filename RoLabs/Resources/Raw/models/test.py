import cv2
import onnxruntime as ort
import numpy as np

class BoundBox:
    def __init__(self, xmin, ymin, xmax, ymax, c=None, classes=None):
        self.xmin = xmin
        self.ymin = ymin
        self.xmax = xmax
        self.ymax = ymax
        
        self.c = c
        self.classes = classes

        self.label = -1
        self.score = -1

    def get_label(self):
        if self.label == -1:
            self.label = np.argmax(self.classes)
        return self.label
    
    def get_score(self):
        if self.score == -1:
            self.score = self.classes[self.get_label()]
        return self.score

# Anchors and labels as per your specification
anchors = [
    (1.08, 1.19), (3.42, 4.41), (6.63, 11.38),
    (9.42, 5.11), (16.62, 10.52)
]

labels = [
    "aeroplane", "bicycle", "bird", "boat", "bottle",
    "bus", "car", "cat", "chair", "cow",
    "diningtable", "dog", "horse", "motorbike", "person",
    "pottedplant", "sheep", "sofa", "train", "tvmonitor"
]

def preprocess_image(image, input_shape):
    resized_image = cv2.resize(image, (input_shape[3], input_shape[2]))
    rgb_image = cv2.cvtColor(resized_image, cv2.COLOR_BGR2RGB)
    normalized_image = rgb_image / 255.0
    transposed_image = np.transpose(normalized_image, (2, 0, 1)).astype(np.float32)
    input_data = np.expand_dims(transposed_image, axis=0)
    return input_data

def _sigmoid(x):
    return 1. / (1. + np.exp(-x))

def _softmax(x, axis=-1, t=-100.):
    x = x - np.max(x)
    if np.min(x) < t:
        x = x/np.min(x)*t
    e_x = np.exp(x)
    return e_x / e_x.sum(axis, keepdims=True)

def bbox_iou(box1, box2):
    intersect_w = np.maximum(0, np.minimum(box1.xmax, box2.xmax) - np.maximum(box1.xmin, box2.xmin))
    intersect_h = np.maximum(0, np.minimum(box1.ymax, box2.ymax) - np.maximum(box1.ymin, box2.ymin))
    intersect = intersect_w * intersect_h

    w1, h1 = box1.xmax - box1.xmin, box1.ymax - box1.ymin
    w2, h2 = box2.xmax - box2.xmin, box2.ymax - box2.ymin

    union = w1 * h1 + w2 * h2 - intersect

    return intersect / union

def postprocess_output(output, obj_threshold=0.5, nms_threshold=0.3):
    grid_size = output.shape[2]
    num_classes = len(labels)
    
    # Reshape the output to [grid_size, grid_size, 5, 5 + num_classes]
    netout = output.reshape((grid_size, grid_size, 5, 5 + num_classes))
    
    grid_h, grid_w, nb_box = netout.shape[:3]

    boxes = []
    
    # decode the output by the network
    netout[..., 4]  = _sigmoid(netout[..., 4])

    netout[..., 5:] = netout[..., 4][..., np.newaxis] * _softmax(netout[..., 5:])
    netout[..., 5:] *= netout[..., 5:] > obj_threshold
    
    for row in range(grid_h):
        for col in range(grid_w):
            for b in range(nb_box):
                classes = netout[row, col, b, 5:]
                
                if np.sum(classes) > 0:
                    x, y, w, h = netout[row, col, b, :4]

                    x = (col + _sigmoid(x)) / grid_w
                    y = (row + _sigmoid(y)) / grid_h
                    w = anchors[b][0] * np.exp(w) / grid_w
                    h = anchors[b][1] * np.exp(h) / grid_h
                    confidence = netout[row, col, b, 4]
                    
                    box = BoundBox(x-w/2, y-h/2, x+w/2, y+h/2, confidence, classes)
                    boxes.append(box)

    for c in range(num_classes):
        sorted_indices = list(reversed(np.argsort([box.classes[c] for box in boxes])))

        for i in range(len(sorted_indices)):
            index_i = sorted_indices[i]
            
            if boxes[index_i].classes[c] == 0:
                continue
            else:
                for j in range(i + 1, len(sorted_indices)):
                    index_j = sorted_indices[j]
                    
                    if bbox_iou(boxes[index_i], boxes[index_j]) >= nms_threshold:
                        boxes[index_j].classes[c] = 0
                        
    boxes = [box for box in boxes if box.get_score() > obj_threshold]
    
    return boxes

def draw_boxes(image, boxes):
    h, w, _ = image.shape
    for box in boxes:
        x1 = int(box.xmin * w)
        y1 = int(box.ymin * h)
        x2 = int(box.xmax * w)
        y2 = int(box.ymax * h)
        color = (0, 255, 0)
        label = f"{labels[box.get_label()]}: {box.get_score():.2f}"
        cv2.rectangle(image, (x1, y1), (x2, y2), color, 2)
        cv2.putText(image, label, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, color, 2)
    return image

def main():
    model_path = 'tinyyolov2-8.onnx'
    session = ort.InferenceSession(model_path)
    input_name = session.get_inputs()[0].name
    input_shape = session.get_inputs()[0].shape

    cap = cv2.VideoCapture(0)

    while True:
        ret, frame = cap.read()
        if not ret:
            break

        input_data = preprocess_image(frame, input_shape)
        print(input_data.shape)
        output = session.run(None, {input_name: input_data})
        
        boxes = postprocess_output(output[0])

        if len(boxes) == 0:
            print("No boxes detected.")
        
        image_with_boxes = draw_boxes(frame, boxes)
        cv2.imshow("YOLOv2 Detection", image_with_boxes)

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    cap.release()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    main()
import cv2
import numpy as np

def letterbox_image(image, size):
    '''Resize image with unchanged aspect ratio using padding'''
    ih, iw = image.shape[:2]
    w, h = size
    scale = min(w/iw, h/ih)
    nw = int(iw * scale)
    nh = int(ih * scale)

    image_resized = cv2.resize(image, (nw, nh), interpolation=cv2.INTER_CUBIC)
    new_image = np.full((h, w, 3), 128, dtype=np.uint8)
    x_offset = (w - nw) // 2
    y_offset = (h - nh) // 2
    new_image[y_offset:y_offset + nh, x_offset:x_offset + nw] = image_resized
    return new_image

def preprocess(img):
    model_image_size = (416, 416)
    boxed_image = letterbox_image(img, model_image_size)
    image_data = np.array(boxed_image, dtype='float32')
    image_data /= 255.0
    image_data = np.transpose(image_data, [2, 0, 1])
    image_data = np.expand_dims(image_data, 0)
    return image_data

# Example usage
img_path = 'your_image_path_here.jpg'  # Replace with your image path
image = cv2.imread(img_path)
image_data = preprocess(image)
image_size = np.array([image.shape[0], image.shape[1]], dtype=np.int32).reshape(1, 2)

print(image_data.shape)
print(image_size)
