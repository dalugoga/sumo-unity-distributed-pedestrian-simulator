#!/usr/bin/env python
# Eclipse SUMO, Simulation of Urban MObility; see https://eclipse.org/sumo
# Copyright (C) 2009-2019 German Aerospace Center (DLR) and others.
# This program and the accompanying materials
# are made available under the terms of the Eclipse Public License v2.0
# which accompanies this distribution, and is available at
# http://www.eclipse.org/legal/epl-v20.html
# SPDX-License-Identifier: EPL-2.0

# @file    runner.py
# @author  Lena Kalleske
# @author  Daniel Krajzewicz
# @author  Michael Behrisch
# @author  Jakob Erdmann
# @date    2009-03-26

"""
Tutorial for traffic light control via the TraCI interface.
This scenario models a pedestrian crossing which switches on demand.
"""
from __future__ import absolute_import
from __future__ import print_function

import os
import sys
import optparse
import subprocess
import time

import threading

from pynput.keyboard import Key, Listener

import json

velx = -1
vely = 0

# the directory in which this script resides
THISDIR = os.path.dirname(__file__)


# we need to import python modules from the $SUMO_HOME/tools directory
# If the the environment variable SUMO_HOME is not set, try to locate the python
# modules relative to this script
if 'SUMO_HOME' in os.environ:
    tools = os.path.join(os.environ['SUMO_HOME'], 'tools')
    sys.path.append(tools)
else:
    sys.exit("please declare environment variable 'SUMO_HOME'")
import traci  # noqa
from sumolib import checkBinary  # noqa
import randomTrips  # noqa


import time
import zmq

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

class Object(dict):
  def __init__(self, name, x, y, angle, edge=""):
    dict.__init__(self, name = name, x = x, y = y, angle = angle, edge = edge)

class Data:
    def __init__(self, persons, vehicles):
        self.persons = persons
        self.vehicles = vehicles

def buildDataList(list, type):
    return_list = []
    for item in list:
        x = 0
        y = 0
        angle = 0
        if type == "vehicles":
            x, y = traci.vehicle.getPosition(item)
            angle = traci.vehicle.getAngle(item)
        else:
            x, y = traci.person.getPosition(item)
            angle = traci.person.getAngle(item)
        return_list.append(Object(item, x, y, angle))
    return return_list


def sumoDataToJSON():
    vehicles = buildDataList(traci.vehicle.getIDList(), "vehicles")
    persons = buildDataList(traci.person.getIDList(), "persons")
    #return json.dumps({"vehicles": dic_vehicles, "pedestrians": dic_persons})
    data = Data(persons, vehicles)
    return json.dumps(data.__dict__)

    

def handleComms(last_subject):
    message = socket.recv()
    print(message)
    subject = json.loads(message)

    if last_subject.get("edge") != subject.get("edge") and subject.get("edge") != "":
        traci.person.removeStages("subject")
        traci.simulationStep()
        traci.person.add("subject", subject.get("edge"), 1)
        traci.person.appendWalkingStage("subject", [subject.get("edge"), "DRR"], 1)
        traci.simulationStep()

    #Working with keepRpute = 3 and edge = ""
    #traci.person.moveToXY("subject", "", subject.get("x"), subject.get("y"), angle=subject.get("angle"), keepRoute=3)
    traci.person.moveToXY("subject", "", subject.get("x"), subject.get("y"), angle=subject.get("angle"), keepRoute=3)
    
    send = sumoDataToJSON()
    socket.send_string(send)

    return subject


def run():
    global velx
    global vely
    mili = 0

    #traci.vehicle.add("corgi", "route_0", typeID="PedCar")
    traci.simulationStep()

    while traci.simulation.getMinExpectedNumber() > 0:
        if int(round(time.time() * 1000)) - mili > 11:
            mili = int(round(time.time() * 1000))
            #if velx != 0 or vely != 0:
                #traci.vehicle.moveToXY("corgi", "", 0, traci.vehicle.getPosition("corgi")[0] + (velx / 100), traci.vehicle.getPosition("corgi")[1] + (vely / 100), angle=90, keepRoute=2)
                            #moveToXY(self, vehID, edgeID, lane, x, y, angle=-1073741824.0, keepRoute=1)
            traci.simulationStep()
        

        

def get_options():
    """define options for this script and interpret the command line"""
    optParser = optparse.OptionParser()
    optParser.add_option("--nogui", action="store_true",
                         default=False, help="run the commandline version of sumo")
    options, args = optParser.parse_args()
    return options


def on_press(key):
    global vely
    global velx
    key_press = key
    #print("PRESSED", key_press)
    if key_press == Key.esc:
        return False
    if key_press == Key.right:
        vely = 0
        velx = 1
    if key_press == Key.left:
        vely = 0
        velx = -1
    if key_press == Key.up:
        vely = 1
        velx = 0
    if key_press == Key.down:
        vely = -1
        velx = 0


def run_listener():
    with Listener(on_press=on_press) as listener:
        listener.join()

# this is the main entry point of this script
if __name__ == "__main__":

    os.chdir(os.path.dirname(sys.argv[0]))
    # load whether to run with or without GUI
    options = get_options()

    # this script has been called from the command line. It will start sumo as a
    # server, then connect and run
    if options.nogui:
        sumoBinary = checkBinary('sumo')
    else:
        sumoBinary = checkBinary('sumo-gui')

    net = 'custom.net.xml'
    # build the multi-modal network from plain xml inputs
    """subprocess.call([checkBinary('netconvert'),
                     #'-c', os.path.join('data', 'pedcrossing.netccfg'),
                     '--output-file', net],
                    stdout=sys.stdout, stderr=sys.stderr)
    """

    # generate the pedestrians for this simulation
    randomTrips.main(randomTrips.get_options([
        '--net-file', net,
        '--output-trip-file', 'pedestrians.trip.xml',
        '--seed', '42',  # make runs reproducible
        '--pedestrians',
        '--prefix', 'ped',
        # prevent trips that start and end on the same edge
        '--min-distance', '1',
        '--trip-attributes', 'departPos="random" arrivalPos="random"',
        '--binomial', '4',
        '--period', '3']))


    thread2 = threading.Thread(target=run_listener, args=())
    thread2.start()

    # this is the normal way of using traci. sumo is started as a
    # subprocess and then the python script connects and runs
    traci.start([sumoBinary, '-c', 'run.sumocfg', '--step-length', '0.011'])

    run()
