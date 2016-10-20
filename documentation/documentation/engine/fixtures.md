<!--Title:Fixtures-->
<!--Url:fixtures-->

<div class="alert alert-warning" role="alert"><strong>Warning!</strong> Only one instance of each Fixture class is created and shared across all specification executions in a Storyteller process. This is a breaking change from earlier version. </div>

The specification language in Storyteller starts with implementations of the `Fixture` class that implement and expose the individual grammars.

A `Fixture` class has to be a public, concrete class that inherits from `Storyteller.Fixture` like the class shown below:

<[sample:sample-fixture]>

### Some things of note:
* The idiom is to name the Fixture classes [Something]Fixture, where "Something" would be the default title.
* You can use the `Title` property of a Fixture class to override the title that gets displayed in the generated html. Otherwise, the default is to take the name of the Fixture class and remove the word "Fixture."
* Grammars can be added programmatically in the constructor function, from methods that return `IGrammar`, and from methods that have either a `Void` signature or return a single value.

<div class="alert alert-info" role="alert"><strong>Info</strong> The initial project team that developed and used Storyteller felt that it was much easier to trace the functionality of the Fixture language to the right place in the code if you primarily used individual methods on the Fixture classes instead of explicitly adding grammars in the constructor function of a Fixture. </div>

## Fixture and Grammars

The `Fixture` class holds its grammar objects in a hash that's exposed as an indexer on the object:

<[sample:fixture-grammars]>

You can both access grammars by name and programmatically add grammar objects to this hashed collection with the C# syntax `this["grammar key"] = myGrammar`. 

<div class="alert alert-info" role="alert"><strong>Info</strong> As of 3.0, Storyteller is finally smart enough to build named grammars on demand if they are missing, so there's no longer any concern about how the grammars are ordered in the Fixture class.</div>

## Adding Grammars by Method

Fixture objects find their grammars by reflecting over its `Type` and looking for any public method whose name matches the requested grammar key. A method that returns an `IGrammar` object will be executed and the resulting IGrammar object is exposed on the Fixture with the name of the method. See the example below:

<[sample:grammar-from-igrammar-return]>

For methods that either have a `Void` signature or return a value that's not `IGrammar`, a Fixture object will create a _Sentence_ grammar that will call that method on the Fixture object when it executes a specification.

See <[linkto:documentation/engine/grammars/sentences]> for more information.



## Accessing the Context

The `ISpecContext` object for the currently executing specification is exposed on Fixture objects through its `Context` property. The following sample is taken from a Fixture used in Storyteller's internal specifications that tests the documentation generation:

<[sample:getting-context-from-fixture]>

Note that the `Context` property is set during execution before any `SetUp` method or Grammar's from the Section are called. **Only use the `Context` property and never cache the `ISpecContext` in a private field to avoid accessing stale data from a previous specification**.


## Retrieving Application Services

Storyteller supplies a lightweight abstraction to provide access to running services in the system under test via this syntax taken from one of the specifications for Storyteller itself:

<[sample:retrieving-service-in-fixture]>

The `GetService<T>` method delegates to the `IExecutionContext` method of the same name that is built from the system under test for each specification execution. See <[linkto:documentation/engine/system_under_test]> for more information.


## The Current Specification

Storyteller has been used on a couple systems that involved reading and working with textual files. While the Storyteller team usually prefers to do define all specification setup data in the specification itself, sometimes we have opted to use the raw textual files. What we wanted to do was simply put the specification input files side by side to wherever the actual Storyteller specification file is persisted and then read files in from relative paths later. That was not really possible in early Storyteller, but is now.

The currently executing specification is reachable inside any `Fixture` class from this code:

<[sample:use-specification-from-fixture]>

An object of the `Specification` class contains all the information and expression of that specification, but also "knows" where it is persisted on the file system.

The Storyteller team theorizes that exposing the current `Specification` may be valuable to "warm up" or parallelize specification execution for better throughput, but we have not yet exploited this.



## SetUp() and TearDown()

As the names imply, the `SetUp()` and `TearDown` methods are executed before and after any steps when a section using a Fixture executes. Use these methods to do any kind of quiet mechanical actions. Examples from past usage include:

* Using SetUp() to navigate a browser to a certain Url when testing browser applications
* Using TearDown() to commit a transaction in Fixtures that primarily setup system state
* Storing information in the Context for another Fixture



## Hidden Grammars and Fixtures

You will occasionaly want to build a grammar that will be used by another grammar, but shouldn't be exposed in the Storyteller editor as a valid choice. In that case, simply mark a grammar with the `[Hidden]` attribute as shown below:

<[sample:hiding-a-grammar]>

In the same token, if you have a `Fixture` class than will be used as an embedded section, but never by itself as a top level section, decorate a `Fixture` class with the `[Hidden]` attribute as shown below:

<[sample:hidden-fixture]>
