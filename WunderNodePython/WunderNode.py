#!/usr/bin/env python
# This is WunderNode Python Edition

import WunderLayer
import WunderPackets

class SubscribedType:
	def __init__(self, fname, ftype):
		self.Name = fname
		self.Type = ftype

class WunderNode(WunderLayer.WunderLayer):
	def __init__(self,id,baddr):
		WunderLayer.WunderLayer.__init__(self,id,baddr)
		self.SendOnline()
		self.FeatureList = {}
		self.FeatureSubscribers = {}
		self.SubscribedFeatures = {}
		
	def AddFeature(self,fname,ftype,fio):
		#featurelist map has a feature
		self.FeatureList[fname] = WunderPackets.StandardFeature(fname,ftype,fio)
		#feature subscriber map has a list
		self.FeatureSubscribers[fname] = []
		
	def UpdateFeature(self, fname, fdata):
		if fname in self.FeatureList.keys():
			if len(self.FeatureSubscribers[fname]) > 0:
				sf = self.FeatureList[fname]
				self.SendFeatureUpdate("", fname, sf.FeatureBaseType, fdata)
				
	def SubscribeToFeature(self, receiver, fname, ftype):
		s = SubscribedType(fname,ftype)
		if receiver in self.SubscribedFeatures:
			self.SubscribedFeatures[receiver].append(s)
		else:
			self.SubscribedFeatures[receiver] = []
			self.SubscribedFeatures[receiver].append(s)
		WunderLayer.WunderLayer.SendFeatureSubscribe(receiver,fname)

	def IsSubscribedToFeature(self, sender, fname):
		if sender in self.SubscribedFeatures:
			for f in self.SubscribedFeatures[sender]:
				if f.Name == fname:
					return True
		return False
		
	def CommandFeature(self, receiver, fname, fdata):
		if receiver in self.SubscribedFeatures:
			for f in self.SubscribedFeatures[receiver]:
				if f.Name == fname:
					self.SendFeatureCommand(receiver, fname, f.Type, fdata)
					return

	def ProcessDescribe(self, basepacket, rawbytes):
		if basepacket.ReceiverID == self.Identifier:
			self.SendDesciption(basepacket.SenderID, self.FeatureList.values())
			
	def ProcessSubscribe(self, basepacket, rawbytes):
		if basepacket.ReceiverID == self.Identifier:
			fp = WunderPackets.FeaturePacket()
			fp.InitFromPacket(rawbytes)
			if fp.FeatureName in self.FeatureSubscribers.keys():
				if fp.SenderID not in self.FeatureSubscribers[fp.FeatureName]:
					self.FeatureSubscribers[fp.FeatureName].append(fp.SenderID)
	
	def ProcessFeatureUpdate(self, basepacket, rawbytes):
		uBlock = WunderPackets.FeaturePacket()
		uBlock.InitFromPacket(rawbytes)
		if IsSubscribedToFeature(uBlock.SenderID, uBlock.FeatureName):
			for callback in self._UpdatePacketEventListeners:
				callback(uBlock)
				
	def ProcessFeatureCommand(self, basepacket, rawbytes):
		if basepacket.ReceiverID == self.Identifier:
			uBlock = WunderPackets.FeaturePacket()
			uBlock.InitFromPacket(rawbytes)
			if uBlock.FeatureName in self.FeatureList:
				for callback in self._CommandPacketEventListeners:
					callback(uBlock)