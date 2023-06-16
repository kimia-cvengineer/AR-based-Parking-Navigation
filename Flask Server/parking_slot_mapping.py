import numpy as np


# Known slots in local coordinates and their corresponding geographical coordinates
# # E 2, 6, 8, 4, 5, 10, 14, 16, 12, 13
# known_slots_local = [(1199, ?), (638, 512), (34.4171164, -119.8452943),
#                    (34.41707, -119.8454458), (34.41707, -119.8454458), (34.4171164, -119.8452943),
#                    (34.4170637, -119.8453056), (34.4170637, -119.8453056), (34.417035, -119.8455316),
#                    (34.41702, -119.8454193)]  # Example local coordinates of known slots
#
# known_slots_geo = [(34.41707, -119.8454458), (34.41707, -119.8454458), (34.4171164, -119.8452943),
#                    (34.41707, -119.8454458), (34.41707, -119.8454458), (34.4171164, -119.8452943),
#                    (34.4170637, -119.8453056), (34.4170637, -119.8453056), (34.417035, -119.8455316),
#                    (34.41702, -119.8454193)]  # Example geographical coordinates of known slots

class GeoMapper:
    # E 2, 6, 3, 9, 5, 7, 14, 15, 13
    _known_slots_local = [(1199, 349), (638, 512), (983, 421),
                          (670, 716), (1110, 533), (284, 597),
                          (716, 307), (263, 392), (1236, 237), ]  # Example local coordinates of known slots

    _known_slots_geo = [(34.41707, -119.8454458), (34.41707, -119.8454458), (34.41705, -119.8455511),
                        (34.4171164, -119.8452943), (34.41707, -119.8454458), (34.4171164, -119.8452943),
                        (34.4170637, -119.8453056), (34.4170637, -119.8453056),
                        (34.41702, -119.8454193)]  # Example geographical coordinates of known slots

    def __init__(self):
        # Construct the matrices A and B for the linear least squares method
        A = np.array(GeoMapper._known_slots_local)
        B = np.array(GeoMapper._known_slots_geo)

        # Calculate the transformation matrix using linear least squares
        self.transform_matrix, _, _, _ = np.linalg.lstsq(A, B, rcond=None)

    # Function to map local coordinates to geographical coordinates
    def map_local_to_geo(self, coordinate: tuple) -> tuple:
        local_x, local_y = coordinate
        local_coords = np.array([local_x, local_y])
        geo_coords = np.dot(local_coords, self.transform_matrix)
        return geo_coords[0], geo_coords[1]  # Return latitude and longitude
