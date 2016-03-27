#!/usr/bin/env python
# This is WunderLayerTester Python Edition

import WunderLayer
import WunderPackets


testWunderLayer = WunderLayer.WunderLayer('t3')

wait = raw_input('Any Key to Stop: ')

testWunderLayer.Disconnect()