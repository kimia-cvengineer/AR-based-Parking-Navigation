import threading

from flask import Flask, jsonify, request

from ParkingSlot import ParkingSlot
from detecting_parking_slots import ParkingSlotDetector
from utils import get_current_datetime

app = Flask(__name__)
lock = threading.Lock()

PARKING_NAME = "Central Parking II"
VIDEO_PATH = "./Recordings/video_2023-06-01_18-29-47.mp4"

parking_detector = ParkingSlotDetector(video_path=VIDEO_PATH)


@app.route('/slots', methods=['GET'])
def slots_endpoint():
    global parking_detector
    status = None if request.args.get("status") is None else request.args.get("status").lower() == "true"

    with lock:
        slots = parking_detector.get_slots()
        output = [slot.to_json() for slot in ParkingSlot.get_slots_with_status(slots, status)]

    return jsonify({'date_time': get_current_datetime(), 'parking': PARKING_NAME, 'slots': output})


if __name__ == '__main__':
    parking_detector.start()
    app.run()
    parking_detector.stop()
