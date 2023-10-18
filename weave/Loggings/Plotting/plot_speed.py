import pandas as pd
import plotly.graph_objects as go
from plotly.subplots import make_subplots

# Load the CSV file
file_path = '../speed.csv'
data = pd.read_csv(file_path)

# Convert 'delta_ms' to seconds
data['delta_s'] = data['delta_ms'] / 1000.0

# Create subplots without shared x-axis
fig = make_subplots(rows=2, cols=1, subplot_titles=('Speed', 'Turn Radius'))

# Add Speed trace to the first subplot
fig.add_trace(
    go.Scatter(x=data['delta_s'], y=data['speed'], name='Speed', line=dict(color='blue')),
    row=1, col=1
)

# Add Turn Radius trace to the second subplot
fig.add_trace(
    go.Scatter(x=data['delta_s'], y=data['turn_radius'], name='Turn Radius', line=dict(color='red')),
    row=2, col=1
)

# Update layout for better readability
fig.update_layout(
    title='Speed and Turn Radius over Time'
)

# Update y-axis labels
fig.update_yaxes(title_text='Speed', row=1, col=1)
fig.update_yaxes(title_text='Turn Radius', row=2, col=1)

# Set the x-axis title for each subplot
fig.update_xaxes(title_text='Time (s)', row=1, col=1)
fig.update_xaxes(title_text='Time (s)', row=2, col=1)

# Show plot
fig.show()
