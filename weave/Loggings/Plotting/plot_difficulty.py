from plotly.subplots import make_subplots
import pandas as pd
import plotly.express as px
import plotly.graph_objects as go

# Load the CSV data from the directory above the script location
data = pd.read_csv('../difficulty.csv')

# Calculate the average score per player count
average_score_per_player = data.groupby('players')['score'].mean()

# Calculate the average time taken per player count (converted to minutes)
average_time_per_player_minutes = data.groupby(
    'players')['time_ms'].mean() / 60000

# Display the distribution of rounds based on player count
rounds_distribution = data.groupby(
    ['players', 'rounds']).size().unstack().fillna(0)

# Plotting the average score per player count
fig1 = px.bar(average_score_per_player.reset_index(),
              x='players',
              y='score',
              labels={'players': 'Player Count', 'score': 'Average Score'},
              title='Average Score Per Player Count')

# Plotting the average time taken per player count
fig2 = px.bar(average_time_per_player_minutes.reset_index(),
              x='players',
              y='time_ms',
              labels={'players': 'Player Count',
                      'time_ms': 'Average Time (in minutes)'},
              title='Average Time (in minutes) Per Player Count')

# Displaying the distribution of rounds based on player count
fig3 = px.bar(rounds_distribution.reset_index().melt(id_vars='players', var_name='rounds', value_name='count'),
              x='players',
              y='count',
              color='rounds',
              labels={'players': 'Player Count', 'count': 'Number of Games'},
              title='Distribution of Rounds Based on Player Count')

# Display plots in the same column

fig = make_subplots(rows=3, cols=1, subplot_titles=("Average Score Per Player Count",
                    "Average Time (in minutes) Per Player Count", "Distribution of Rounds Based on Player Count"))

for trace in fig1['data']:
    fig.add_trace(trace, row=1, col=1)

for trace in fig2['data']:
    fig.add_trace(trace, row=2, col=1)

for trace in fig3['data']:
    fig.add_trace(trace, row=3, col=1)

fig.update_layout(height=900, showlegend=False)
fig.show()

# Inspirational Quote
print("Dreams and dedication are a powerful combination. - William Longgood")

# ASCII Signature from ChatGPT
print('''
   _____ _           _   _____ _____ 
  / ____| |         | | |_   _|  __ \\
 | |    | |__   __ _| |_| | | | |__) |
 | |    | '_ \ / _` | __| | | |  _  / 
 | |____| | | | (_| | |_| |_| | | \ \ 
  \_____|_| |_|\__,_|\__|_____|_|  \_\

          - Signed, ChatGPT
''')
