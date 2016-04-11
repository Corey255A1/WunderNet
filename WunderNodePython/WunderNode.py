#!/usr/bin/env python
# This is WunderNode Python Edition

import WunderLayer
import WunderPackets

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
		
	def SubscribeToFeature(self, receiver, fname):
		if receiver in self.SubscribedFeatures:
			self.SubscribedFeatures[receiver].append(fname)
		else:
			self.SubscribedFeatures[receiver] = []
			self.SubscribedFeatures[receiver].append(fname)
		WunderLayer.WunderLayer.SendFeatureSubscribe(receiver,fname)
	def UpdateFeature(self, fname, fdata):
		if fname in self.FeatureList.keys():
			if len(self.FeatureSubscribers[fname]) > 0:
				sf = self.FeatureList[fname]
				self.SendFeatureUpdate("", fname, sf.FeatureBaseType, fdata)
	
	def IsSubscribedToFeature(self, sender, fname):
		if sender in self.SubscribedFeatures:
			if fname in self.SubscribedFeatures[sender]:
				return True
	
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
		uBlock.InitFromPacket(rawdata)
		if IsSubscribedToFeature(uBlock.SenderID, uBlock.FeatureName):
			for callback in self._UpdatePacketEventListeners:
				callback(uBlock)