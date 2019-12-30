# This script generates the scoring file
# with the init and run functions needed to 
# operationalize the anomaly detection sample
# Code was only slightly modified based on tutorial provided by Azure at https://github.com/Azure/ai-toolkit-iot-edge/tree/master/IoT%20Edge%20anomaly%20detection%20tutorial

import pickle
import json
import pandas
from sklearn.externals import joblib
from sklearn.linear_model import Ridge
from azureml.core.model import Model

def init():
    global model
    # this is a different behavior than before when the code is run locally, even though the code is the same.
    model_path = Model.get_model_path('model.pkl')
    # deserialize the model file back into a sklearn model
    model = joblib.load(model_path)

# note you can pass in multiple rows for scoring
def run(input_str):
    try:
        input_json = json.loads(input_str)
        input_df = pandas.DataFrame([[input_json['fromuser'],input_json['touser']]])
        pred = model.predict(input_df)
        print("Prediction is ", pred[0])
    except Exception as e:
        result = str(e)
        
    if pred[0] == 1:
        input_json['anomaly']=True
    else:
        input_json['anomaly']=False
        
    return [json.dumps(input_json)]