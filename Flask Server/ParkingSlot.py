from hashlib import sha256


class ParkingSlot:

    def __init__(self, latitude: float, longitude: float, is_empty: bool = False):
        _str = str(latitude) + str(longitude)
        self.slot_id = sha256(_str.encode('utf-8')).hexdigest()
        self.latitude = latitude
        self.longitude = longitude
        self.is_empty = is_empty

    def to_json(self):
        return {
            'id': self.slot_id,
            'latitude': self.latitude,
            'longitude': self.longitude,
            'is_empty': self.is_empty,
        }

    def __str__(self):
        return f'id: {self.slot_id}, latitude: {self.latitude}, longitude: {self.longitude}, is_empty: {self.is_empty}'

    @classmethod
    def get_slots_with_status(cls, slots: list, status: bool or None):
        if status is None:
            return slots
        return [slot for slot in slots if slot.is_empty is status]
