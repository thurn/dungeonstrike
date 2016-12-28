Logging helper is a very simple class to allow you to easily log multiple values to the Unity Console Window.

To use, it is as easy as including the clas anywhere in your project and making a function call.

Now, from within any MonoBehaviour you can call
this.LogValues(); 
to log all of the values you defined in your script.
yes you need to include the "this".

Alternatively, you can log specific calues like this.
this.LogValues(new{name, health, ammo, speed});
these values can be member variables, or local variables.

Lastly, if you have another MonoBehavior as a member such as you can log it's members.
otherMonoBehaviour.LogValues();

An example of the log output for this.LogValues(new{Name, health, ammo, speed}); called from a MonoBehavior called TestLoggin placed on the Main Camera

TestLogging on Main Camera properties log.

Name=Joe
health=10
ammo=5
speed=100

UnityEngine.Debug:Log(Object, Object)
LoggingHelper:LogValues(MonoBehaviour, <>__AnonType0`4) (at Assets/LoggingHelper/Scripts/LoggingHelper.cs:25)
TestLogging:Start() (at Assets/LoggingHelper/TestLogging.cs:19)