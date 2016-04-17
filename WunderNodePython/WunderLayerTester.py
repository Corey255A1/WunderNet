#!/usr/bin/env python
# This is WunderLayerTester Python Edition

import WunderNode
import WunderPackets
import thread
import time
from random import randint

threadGo = True

def PrintStringData(sdBlock):
	print sdBlock.SenderID + ": " + sdBlock.Data

def FeatureData(fupacket):
	print fupacket.SenderID + ":" + fupacket.FeatureName + ":" + fupacket.Data

def CommandData(fupacket):
	print "Command: " + fupacket.SenderID + ":" + fupacket.FeatureName + ":" + fupacket.Data
		
testWunderNode = WunderNode.WunderNode('t5','192.168.0.255')

testWunderNode.RegisterForStringDataPackets(PrintStringData)
testWunderNode.RegisterForFeaturePackets(FeatureData)
testWunderNode.RegisterForFeatureCommands(CommandData)
testWunderNode.AddFeature('FrontProps',WunderPackets.FeatureBaseTypes['INT'],WunderPackets.FeatureIOTypes['OUTPUT'])
testWunderNode.AddFeature('RearProps',WunderPackets.FeatureBaseTypes['INT'],WunderPackets.FeatureIOTypes['OUTPUT'])
testWunderNode.AddFeature('FrontSonar',WunderPackets.FeatureBaseTypes['INT'],WunderPackets.FeatureIOTypes['INPUT'])

def UpdateFeature(*args):
	while threadGo:
		testWunderNode.UpdateFeature('FrontSonar', randint(0,25))
		time.sleep(1)

		
thread.start_new_thread(UpdateFeature,('',''))
cmd = raw_input('')
while not cmd == 'stop':
	cmd = raw_input('')

threadGo = False
testWunderNode.Disconnect()