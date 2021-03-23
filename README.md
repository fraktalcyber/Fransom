# README #

![Fransom Logo](/images/Fransom.png)
## FRANSOM - Fraktal Ransomware Emulator. 

**DO NOT RUN IN CRITICAL / PRODUCTION ENVIRON.**

Fransom incorporates code and functionality from the following open source offensive .NET tools:

 * Process injection mechanisms are from [C# Memory Injection Examples project](https://github.com/pwndizzle/c-sharp-memory-injection) by [pwndizzle](https://twitter.com/pwndizzle). 
 * LSASS dumping implementation is from https://github.com/GhostPack/SharpDump by [harmj0y](https://twitter.com/harmj0y). SharpDump is licensed under the BSD 3-Clause license, available [here](https://raw.githubusercontent.com/GhostPack/SharpDump/master/LICENSE).
 * calc.exe shellcode is from [win-exec-calc-shellcode](https://github.com/peterferrie/win-exec-calc-shellcode) by Berend-Jan "SkyLined" Wever and Peter Ferrie. [Copyright](https://raw.githubusercontent.com/peterferrie/win-exec-calc-shellcode/master/COPYRIGHT.txt).


## What is this repository for? ##

* Command-line executable that will emulate common ransomware functions.
* Version: 0.6
* Type "help" (without quotes) inside the emulator prompt to get a list of all the available options.

## How do I get set up? ##

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
