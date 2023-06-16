import threading
import time

import cv2
import torch
from torchvision import transforms

from ParkingSlot import ParkingSlot
from inference import get_mask_rcnn_model as MaskRCNN
from inference import predict
from parking_slot_mapping import GeoMapper
from utils import *


class ParkingSlotDetector(threading.Thread):
    MODEL_WEIGHTS = "weights_epoch_26.pt"

    def __init__(self, video_path=0, capture_interval: int = 30, debug: bool = True):
        threading.Thread.__init__(self)
        self.debug = debug
        self.capture_interval = capture_interval
        # Initialize the video capture object
        self.cap = cv2.VideoCapture(video_path)

        # Initialize a transform to convert the captured frame to a PIL image
        self.transform = transforms.ToTensor()

        # Set model's device
        self.device = torch.device('cpu')

        # create model
        self.model = MaskRCNN(weights=ParkingSlotDetector.MODEL_WEIGHTS, device=self.device)

        self.preds = {}

    def run(self) -> None:
        while True:
            ret, frame = self.cap.read()
            if ret:
                self.__log("Reading")
                frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                tensor_image = self.transform(frame) * 255
                self.__log("Detecting")
                # inference
                self.preds = predict(model=self.model, img=tensor_image, device=self.device)
                self.preds = self.filter_model_output(output=self.preds, score_threshold=0.4)[0]
                self.__log("Done")
                # Wait for the capture interval before capturing the next frame
                time.sleep(self.capture_interval)

    def get_slots(self) -> list:
        slots = []
        if self.preds is None or self.preds.get('boxes') is None or self.preds.get('labels') is None:
            return slots

        bboxes = self.preds.get('boxes')
        labels = self.preds.get('labels')
        self.__log("Centralizing")

        bboxes_centers = find_centers(bboxes)
        for center, label in zip(bboxes_centers, labels):
            lat, lon = GeoMapper().map_local_to_geo(center)
            slot = ParkingSlot(lat, lon, label.item() == 1)
            slots.append(slot)

        return slots

    def filter_model_output(self, output, score_threshold):
        filtred_output = list()
        for image in output:
            filtred_image = dict()
            for key in image.keys():
                filtred_image[key] = image[key][image['scores'] >= score_threshold]
            filtred_output.append(filtred_image)
        return filtred_output


    def stop(self):
        self.cap.release()

    def __log(self, msg):
        if self.debug:
            print(msg)
