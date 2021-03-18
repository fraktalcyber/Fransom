# README #

FRANSOM - Fraktal Ransomware Emulator. **DO NOT RUN IN CRITICAL / PRODUCTION ENVIRON.**

### What is this repository for? ###

* Command-line executable that will emulate common ransomware functions.
* Version: 0.6
* Type "help" (without quotes) inside the emulator prompt to get a list of all the available options.

### How do I get set up? ###

* Open the .sln solution file in Visual Studio (developed with VS 2019, and VS 2017).
* To add/modify functions:
	* Add the corresponding option in the "options" class;
	* Add the corresponding option in the "Run" function;
	* Add the corresponding option in the switch inside the main function;
	* Create the corresponding function.
* Dependencies:
	* ILMerge NuGet package.
	* ILMerge.MSBuild.Tasks NuGet package.
	* Figgle NuGet package.
