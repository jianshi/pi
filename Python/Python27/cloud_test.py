import json
from os.path import join, dirname
from os import environ
from watson_developer_cloud import VisualRecognitionV3 as VisualRecognition

visual_recognition = VisualRecognition('2016-05-20', api_key='c9b2fc34019deb10332121aa27852370b6a9548f')

print(json.dumps(visual_recognition.classify(images_url="https://www.ibm.com/ibm/ginni/images/ginni_bio_780x981_v4_03162016.jpg"), indent=2))
