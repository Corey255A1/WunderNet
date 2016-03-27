#!/usr/bin/env python
# This is WunderLayerTester Python Edition

import WunderLayer
import WunderPackets

def PrintStringData(sdBlock):
	print sdBlock.SenderID + ": " + sdBlock.Data

testWunderLayer = WunderLayer.WunderLayer('t2','192.168.0.255')

testWunderLayer.SendOnline()
testWunderLayer.RegisterForStringDataPackets(PrintStringData)

testWunderLayer.AddFeature(WunderPackets.StandardFeature('FrontProps',WunderPackets.FeatureBaseTypes['INTVAL'],WunderPackets.FeatureIOTypes['OUTPUT']))
testWunderLayer.AddFeature(WunderPackets.StandardFeature('RearProps',WunderPackets.FeatureBaseTypes['INTVAL'],WunderPackets.FeatureIOTypes['OUTPUT']))
testWunderLayer.AddFeature(WunderPackets.StandardFeature('FrontSonar',WunderPackets.FeatureBaseTypes['INTVAL'],WunderPackets.FeatureIOTypes['INPUT']))

cmd = raw_input('')
while not cmd == 'stop':
	cmd = raw_input('')

testWunderLayer.Disconnect()