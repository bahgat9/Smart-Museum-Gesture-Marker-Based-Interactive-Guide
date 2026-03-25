from dollarpy import Recognizer, Template, Point
from gesture_templates import gesture_dataset


def to_dollar_points(points):
    return [Point(x, y, 1) for x, y in points]


def build_templates():
    templates = []

    for gesture_name, samples in gesture_dataset.items():
        for sample in samples:
            templates.append(Template(gesture_name, to_dollar_points(sample)))

    return templates


templates = build_templates()
recognizer = Recognizer(templates)


def get_direction(points):
    """
    Determine basic horizontal movement direction
    using first x and last x.
    """
    start_x = points[0][0]
    end_x = points[-1][0]

    if end_x > start_x:
        return "right"
    elif end_x < start_x:
        return "left"
    return "none"


def recognize_gesture(points):
    candidate = to_dollar_points(points)
    result = recognizer.recognize(candidate)

    if isinstance(result, tuple):
        predicted_name = result[0]
    else:
        predicted_name = str(result)

    if predicted_name in ["swipe_left", "swipe_right"]:
        direction = get_direction(points)

        if direction == "left":
            predicted_name = "swipe_left"
        elif direction == "right":
            predicted_name = "swipe_right"

    return {"gesture": predicted_name}