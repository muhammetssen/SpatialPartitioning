import os
import time
import subprocess

epoch_time = int(time.time())

command_string = "tshark -i loopback -t e -l udp portrange 10000-10009 or udp portrange 20000-20009"
command = command_string.split(' ')

if not os.path.exists("PacketLogs") :
  os.mkdir("PacketLogs")

with open(f'./PacketLogs/{epoch_time}.txt', 'w+') as f:
  subprocess.run(command, stdout=f)