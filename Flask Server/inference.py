import matplotlib.pyplot as plt
import torch
import numpy as np
from models import rcnn_fpn as Mask_RCNN
from torchvision.io import read_image

import cv2
import random

CLASS_NAMES = ['Empty', 'Occupied']


def get_mask_rcnn_model(weights, device):
    # set to evaluation mode
    # device = torch.device('cuda') if torch.cuda.is_available() else torch.device('cpu')
    model = Mask_RCNN.create_model().to(device)
    model.load_state_dict(torch.load(weights, map_location=device))
    model.eval()
    return model


def predict(model, img, device):
    # img = read_image(img_path)
    # convert image to float
    img = img.to(torch.float32) / 255
    img.to(device)

    # model = get_model(weights=weights)
    # model.eval()
    return model([img])


def get_prediction(img_path, device):
    return predict(
        model=get_mask_rcnn_model(weights='./Parking_model/weights_epoch_26.pt',
                                  device=device),
        img_path=img_path, device=device)


def get_coloured_mask(mask):
    """
    random_colour_masks
      parameters:
        - image - predicted masks
      method:
        - the masks of each predicted object is given random colour for visualization
    """
    colours = [[0, 255, 0], [0, 0, 255], [255, 0, 0], [0, 255, 255], [255, 255, 0], [255, 0, 255], [80, 70, 180],
               [250, 80, 190], [245, 145, 50], [70, 150, 250], [50, 190, 190]]
    r = np.zeros_like(mask).astype(np.uint8)
    g = np.zeros_like(mask).astype(np.uint8)
    b = np.zeros_like(mask).astype(np.uint8)
    r[mask == 1], g[mask == 1], b[mask == 1] = colours[random.randrange(0, 10)]
    coloured_mask = np.stack([r, g, b], axis=2)
    return coloured_mask


def segment_instance(img_path, confidence=0.5, rect_th=2, text_size=2, text_th=2):
    """
    segment_instance
      parameters:
        - img_path - path to input image
        - confidence- confidence to keep the prediction or not
        - rect_th - rect thickness
        - text_size
        - text_th - text thickness
      method:
        - prediction is obtained by get_prediction
        - each mask is given random color
        - each mask is added to the image in the ration 1:0.8 with opencv
        - final output is displayed
    """
    masks, boxes, pred_cls = get_prediction(img_path, confidence)
    img = cv2.imread(img_path)
    img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    for i in range(len(masks)):
        rgb_mask = get_coloured_mask(masks[i])
        img = cv2.addWeighted(img, 1, rgb_mask, 0.5, 0)
        cv2.rectangle(img, boxes[i][0], boxes[i][1], color=(0, 255, 0), thickness=rect_th)
        cv2.putText(img, pred_cls[i], boxes[i][0], cv2.FONT_HERSHEY_SIMPLEX, text_size, (0, 255, 0), thickness=text_th)
    plt.figure(figsize=(20, 30))
    plt.imshow(img)
    plt.xticks([])
    plt.yticks([])
    plt.show()