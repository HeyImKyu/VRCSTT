# VRCSTT

[![Deploy VRCSTT](https://github.com/HeyImKyu/VRCSTT/actions/workflows/Publish_VRCSTT.yml/badge.svg)](https://github.com/HeyImKyu/VRCSTT/actions/workflows/Publish_VRCSTT.yml)

Welcome to my implementation of a speech-to-text tool for the VRChat textbox.

**Disclaimer:** You will need a Microsoft Azure Speech-Recognition service. 
                You can find infos about it on [Microsoft Azure](https://azure.microsoft.com/de-de/services/cognitive-services/speech-to-text/#overview).\
                This is a project i started for myself, if you wanna use it or play around with it, 
                you are welcome to do so, but it WILL have bugs and i don't know if i'll even continue develpoment here.\
                If you want a program that just works out of the box, check out https://vrcstt.com/.
                
# SETUP

 - Look for the newest release on the [Release Page](https://github.com/HeyImKyu/VRCSTT/releases) and download the .zip
 - Unpack it somewhere
 - Open the file 'VRCSTT.dll.config'
 - Place the SubscriptionKey and the Region of you Azure speech-recognition service inside the 'Value'-fields.
 - Run 'VRCSTT.exe'
 - Set the slider to the time that should be waited before sending the remaining part of a message, if it is too long to be sent at once. (VRChat can only display 144 characters at once)
 - Make sure that OSC is enabled ingame (Circle Menu -> Options -> OSC -> Enabled)

 # SETUP WITH AVATAR PARAMETER

 - Follow the normal setup above
 - Add an ExpressionParameter to your avatar called "StartVoiceRecognition" (boolean, default false)

 Voice recognition will now start any time you set the "StartVoiceRecognition"-Parameter to true.
 You can do that easily by adding a "Button" to your ExpressionMenu that activates this parameter.

 \
 \
 \
 If you have any problems, you can open an issue; don't expect immediate response tho.
