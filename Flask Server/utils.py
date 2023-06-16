from datetime import datetime


def find_centers(bboxes):
    """
    Calculate the center of each bounding boxes
    :param bboxes: a list of 4 items list [[x1, y1 , x2, y2], ...]
    :return: a list of tuples [(x,y), ...]
    """
    return [(int(((box[0] + box[2]) / 2).item()), int(((box[1] + box[3]) / 2).item())) for box in bboxes]


def get_current_datetime(datetime_format: str = '%Y-%m-%d %H:%M:%S') -> str:
    current_datetime = datetime.now()
    formatted_datetime = current_datetime.strftime(datetime_format)
    return formatted_datetime
