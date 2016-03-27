#!/usr/bin/env python
# This is WunderLayer Python Edition

import WunderNet
import WunderPackets

class WunderLayer:
	def __init__(self, id):
		self._Identifier = id
		self._TheNet = WunderNet.WunderNet()
		self._TheNet.RegisterCallback(self.ProcessWunderNet)
		self._TheNet.StartListening(1000)
	
	def Disconnect(self):
		self._TheNet.StopListening()
	
	def ProcessWunderNet(self,data,*args):
		if WunderPackets.CheckHeader(data):
			print 'Got a WunderPacket!!!'
			p = WunderPackets.BasePacket(data)

