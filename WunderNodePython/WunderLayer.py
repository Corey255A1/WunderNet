#!/usr/bin/env python
# This is WunderLayer Python Edition

import WunderNet
import WunderPackets

class WunderLayer:
	def __init__(self, id, baddr):
		self.Identifier = id

		self.TheNet = WunderNet.WunderNet(baddr)
		self.TheNet.RegisterCallback(self.ProcessWunderNet)
		self.TheNet.StartListening(1000)
		
		self._BasePacketEventListeners = []
		self._StringDataPacketEventListeners = []
		self._DescriptionPacketEventListeners = []
		self._UpdatePacketEventListeners = []
		self._CommandPacketEventListeners = []
		
		self._packetFunctions = { 
			WunderPackets.PacketTypes['ONLINE'] : self.ProcessBasePacket,
			WunderPackets.PacketTypes['OFFLINE'] : self.ProcessBasePacket,
			WunderPackets.PacketTypes['DATABLOCK'] : self.ProcessDataBlock,
			WunderPackets.PacketTypes['DISCOVER'] : self.ProcessDiscover,
			WunderPackets.PacketTypes['IDENTIFY'] : self.ProcessBasePacket,
			WunderPackets.PacketTypes['DESCRIBE'] : self.ProcessDescribe,
			WunderPackets.PacketTypes['DESCRIPTION'] : self.ProcessDescription,
			WunderPackets.PacketTypes['UPDATE'] : self.ProcessFeatureUpdate,
			WunderPackets.PacketTypes['COMMAND'] : self.ProcessFeatureCommand,
			WunderPackets.PacketTypes['SUBSCRIBE'] : self.ProcessSubscribe
		}
		
	def RegisterForBasePackets(self, function):
		self._BasePacketEventListeners.append(function)
	
	def RegisterForStringDataPackets(self, function):
		self._StringDataPacketEventListeners.append(function)
		
	def RegisterForDescriptionPackets(self, function):
		self._DescriptionPacketEventListeners.append(function)
	
	def RegisterForFeaturePackets(self, function):
		self._UpdatePacketEventListeners.append(function)
		
	def RegisterForFeatureCommands(self, function):
		self._CommandPacketEventListeners.append(function)
		
	def AddFeature(self, feature):
		self.FeatureList.append(feature)
	
	def SendOnline(self):
		p = WunderPackets.BasePacket()
		p.InitPacket(self.Identifier, '', WunderPackets.PacketTypes['ONLINE'])
		self.TheNet.SendPacket(p.GetBytes())
	
	def SendOffline(self):
		p = WunderPackets.BasePacket()
		p.InitPacket(self.Identifier, '', WunderPackets.PacketTypes['OFFLINE'])
		self.TheNet.SendPacket(p.GetBytes())
	
	def SendIdentify(self, receiver):
		p = WunderPackets.BasePacket()
		p.InitPacket(self.Identifier, receiver, WunderPackets.PacketTypes['IDENTIFY'])
		self.TheNet.SendPacket(p.GetBytes())
		
	def SendDesciption(self, receiver, features):
		p = WunderPackets.DescriptionPacket()
		p.InitPacket(self.Identifier, receiver, features)
		self.TheNet.SendPacket(p.GetBytes())
	
	def SendFeatureUpdate(self, receiver, fname, ftype, fdata):
		p = WunderPackets.FeaturePacket()
		p.InitPacket(self.Identifier, receiver, WunderPackets.PacketTypes['UPDATE'],fname,ftype,fdata)
		self.TheNet.SendPacket(p.GetBytes())
		
	def SendFeatureCommand(self, receiver, fname, ftype, fdata):
		p = WunderPackets.FeaturePacket()
		p.InitPacket(self.Identifier, receiver, WunderPackets.PacketTypes['COMMAND'],fname,ftype,fdata)
		self.TheNet.SendPacket(p.GetBytes())	
	
	def SendFeatureSubscribe(self, receiver, fname):
		p = WunderPackets.FeaturePacket()
		p.InitPacket(self.Identifier, receiver, WunderPackets.PacketTypes['SUBSCRIBE'],fname,0,'')
		self.TheNet.SendPacket(p.GetBytes())
	
	def Disconnect(self):
		self.SendOffline()
		self.TheNet.StopListening()
	
	def ProcessWunderNet(self,data,*args):
		if WunderPackets.CheckHeader(data):
			# print 'WunderData Found'
			p = WunderPackets.BasePacket()
			p.InitFromPacket(data)
			if p.SenderID == self.Identifier:
				return
			# print p.SenderID + ":" + str(p.PacketType)	
			self._packetFunctions[p.PacketType](p, data);
	
	def ProcessBasePacket(self, wpacket, rawdata):
		for callback in self._BasePacketEventListeners:
			callback(wpacket)
	
	def ProcessDiscover(self, wpacket, rawdata):
		self.SendIdentify(wpacket.SenderID)
	
	def ProcessDataBlock(self, wpacket, rawdata):
		if wpacket.ReceiverID == self.Identifier:
			dBlock = WunderPackets.StringDataPacket()
			dBlock.InitFromPacket(rawdata)
			for callback in self._StringDataPacketEventListeners:
				callback(dBlock)
	def ProcessDescribe(self, wpacket, rawdata):
		return False
	
	def ProcessSubscribe(self, wpacket, rawdata):
		return False
		
	def ProcessDescription(self, wpacket, rawdata):
		if wpacket.ReceiverID == self.Identifier:
			for callback in self._DescriptionPacketEventListeners:
				callback(wpacket)
				
	def ProcessFeatureUpdate(self, wpacket, rawdata):
		if wpacket.ReceiverID == self.Identifier:
			uBlock = WunderPackets.FeaturePacket()
			uBlock.InitFromPacket(rawdata)
			for callback in self._UpdatePacketEventListeners:
				callback(uBlock)
				
	def ProcessFeatureCommand(self, wpacket, rawdata):
		if wpacket.ReceiverID == self.Identifier:
			uBlock = WunderPackets.FeaturePacket()
			uBlock.InitFromPacket(rawdata)
			for callback in self._CommandPacketEventListeners:
				callback(uBlock)

