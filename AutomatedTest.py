import csv
import os
import re
import signal
import subprocess
import time
from datetime import datetime
from threading import Timer

import psutil

SERVER_COUNT = 6
CLIENT_COUNT = 2

PIDList = []
PID_Port = {}

tsharkPID = 0
tsharkLines = []

GET_PID_LIST_TIME = 2
KILL_TIME = 20

epoch_time = int(time.time())
filePath = os.getcwd() + "/pid_endpoint.txt"; 

global processCount
processCount = 0

global producedOutputAmount
producedOutputAmount = 0

def GetUnityInstanceCommand(isServer, index):
  if isServer:
    return f"open -n ./SpatialPartitioning.app --args -batchmode -serverIndex {index}".split(" ")
  else:
    return f"open -n ./SpatialPartitioning.app --args -clientIndex {index}".split(" ")

def GetTopCommand():
  command = ["top","-l","30"]

  for pid in PIDList:
    command.append("-pid")
    command.append(str(pid))

  return command

def FillPIDList():
  with open(filePath, "r") as f:
    lines = f.read().splitlines()

    print("lines now")
    print(lines)

    for line in lines:
      tokens = line.split(" ")

      pid = int(tokens[0])
      port = tokens[1]

      PIDList.append(pid)
      PID_Port[pid] = port

    print("PIDList = ")
    print(PIDList)
    print("PID_Port = ")
    print(PID_Port)

  #for proc in psutil.process_iter():
  #  if proc.name() == process_name:
  #    PIDList.append(proc.pid)

  InitializeCSVFiles()
  
def KillAllProcesses():
  for pid in PIDList:
    os.kill(pid, signal.SIGKILL)
  
  os.kill(0, signal.SIGKILL)

def InitializeCSVFiles():
  if not os.path.exists("SummaryLogs"):
    os.mkdir("SummaryLogs")
  
  if not os.path.exists("SummaryLogs/" + str(epoch_time)):
    os.mkdir("SummaryLogs/" + str(epoch_time))

  for pid in PIDList:
    file_name = "SummaryLogs/" + str(epoch_time) + "/" + str(pid) + ".csv"
    with open(file_name, 'w', newline='') as csvfile:
      fieldnames = ['Time (timestamp secs)', 'Bandwidth (kbps)', 'CPU %', 'Memory %']
      writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
      writer.writeheader()

def WriteLineToCSV(pid, cpu, timestamp, bandwidth, mem):
  file_name = "SummaryLogs/" + str(epoch_time) + "/" + str(pid) + ".csv"
  with open(file_name, 'a', newline='') as csvfile:
    fieldnames = ['Time (timestamp secs)', 'Bandwidth (kbps)', 'CPU %', 'Memory %']
    writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
    writer.writerow({'Time (timestamp secs)': timestamp, 'Bandwidth (kbps)': bandwidth, 'CPU %': cpu, 'Memory %': mem})

def ParseTopOutput(topOutput):
  lines = topOutput.splitlines()

  timestampLine = lines[1]
  timestamp = int(time.mktime(datetime.strptime(timestampLine, "%Y/%m/%d %H:%M:%S").timetuple()))

  processLines = lines[len(lines) - processCount:]

  for line in processLines:
    tokens = (' '.join(line.split())).split(" ")

    pid = tokens[0]
    cpu = tokens[2]

    bw_sent = 0
    bw_received = 0

    for line in tsharkLines:
      tsharkTokens = ' '.join(line.split()).split(" ")

      packetTimestamp = int(float(tsharkTokens[1]))
      packetSize = tsharkTokens[6]
      packetFromPort = tsharkTokens[7]
      packetToPort = tsharkTokens[9]
      print(packetTimestamp,packetSize, packetFromPort, packetToPort)

      if packetTimestamp == timestamp:
        if packetFromPort == PID_Port[int(pid)]:
          bw_sent += int(packetSize)
        elif packetToPort == PID_Port[int(pid)]:
          bw_received += int(packetSize)

    mem = tokens[7]

    WriteLineToCSV(pid, cpu, timestamp, str(bw_sent) + "&" + str(bw_received), mem)

# Start of execution
if os.path.exists(filePath):
  os.remove(filePath)
  open(filePath, "x")
else:
  open(filePath, 'x')

# Start capturing packets
tsharkCommand = "tshark -i loopback -t e -l udp portrange 10000-10009 or udp portrange 20000-20009".split(" ")

if not os.path.exists("PacketLogs"):
  os.mkdir("PacketLogs")

tsharkOutputFilePath = f'./PacketLogs/{epoch_time}.txt'
tsharkPID = 0
with open(tsharkOutputFilePath, 'w+') as f:
  tsharkProcess = subprocess.Popen(tsharkCommand, stdout=f)
  tsharkPID = tsharkProcess.pid


# Start servers
for i in range(SERVER_COUNT):
  proc = subprocess.Popen(GetUnityInstanceCommand(True,i))
  proc.pid
  processCount = processCount + 1

# Start clients
for i in range(CLIENT_COUNT):
  subprocess.Popen(GetUnityInstanceCommand(False,i))
  processCount = processCount + 1

print(f"Current process count is : {processCount}")

process_name = "SpatialPartitioning"

time.sleep(5)

FillPIDList()

# Get the top process output
topProcessOutput = subprocess.check_output(GetTopCommand())

# Divide the top command output using regex
regex = r"((Processes:.*\n)(^(?!.*(Processes)).*\n)*)"
topBlocks = []
matches = re.finditer(regex, topProcessOutput.decode("utf-8"), re.MULTILINE)

for matchNum, match in enumerate(matches, start=1):
    topBlocks.append(match.group())

os.kill(tsharkPID, signal.SIGKILL)
tsharkLines = open(tsharkOutputFilePath, "r").readlines()

# Parse top output
for topBlock in topBlocks[1:]:
  ParseTopOutput(topBlock)

KillAllProcesses()
