import pandas as pd
import plotly.express as px

# Load the CSV file
filename = '../speed.csv'
data = pd.read_csv(filename)

# Set the 'id' column as index
data.set_index('id', inplace=True)

# Creating the plot
fig = px.line(data_frame=data.reset_index(), x='id', y=['speed', 'turn_radius'], labels={
    'value': 'Values', 'variable': 'Legend'}, title='Speed and Turn Radius vs ID')

# Show the plot
fig.show()
