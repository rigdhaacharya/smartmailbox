# Load the data set
# Code was only slightly modified based on tutorial provided by Azure at https://github.com/Azure/ai-toolkit-iot-edge/tree/master/IoT%20Edge%20anomaly%20detection%20tutorial
import pandas
import numpy
import pickle
from sklearn import tree
from sklearn.model_selection import train_test_split

temp_data = pandas.read_csv('dat.csv')
temp_data

X, Y = temp_data[['fromuser', 'touser']].values, temp_data['usercategory'].values

# Split data 65%-35% into training set and test set
X_train, X_test, Y_train, Y_test = train_test_split(X, Y, test_size=0.35, random_state=0)

# Change regularization rate and you will likely get a different accuracy.
reg = 0.01

# Train a decision tree on the training set
#clf1 = LogisticRegression(C=1/reg).fit(X_train, Y_train)
clf1 = tree.DecisionTreeClassifier()
clf1 = clf1.fit(X_train, Y_train)
print (clf1)

# Evaluate the test set
accuracy = clf1.score(X_test, Y_test)

print ("Accuracy is {}".format(accuracy))

# Serialize the model and write to disk
f = open('model.pkl', 'wb')
pickle.dump(clf1, f)
f.close()
print ("Exported the model to model.pkl")

# Test the model by importing it and providing a sample data point
print("Import the model from model.pkl")
f2 = open('model.pkl', 'rb')
clf2 = pickle.load(f2)

# Anomaly
X_new = [[9,3]]

print ('New sample: {}'.format(X_new))

pred = clf2.predict(X_new)
print('Predicted class is {}'.format(pred))