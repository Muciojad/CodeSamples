
# Code Samples

Here are 3 simplified code examples from my real-life projects to better show the wide spectrum of my capabilities and proficiency in various Unity areas, instead of just one single sample polished just for shining on public GitHub.

## Inspector Generator
An editor tool window that lets non-programmers (and programmers too) generate inspector classes for components and automatically assign component references to the freshly generated inspector. This tool can speed up the process of creating UI popups and other types of components in Unity.

The tool generates a separate partial class for the inspector with fields that the user added using the editor window. After recompiling, it automatically grabs references for the selected component and assigns them to fields in the inspector.

Tool is made using Odin plugin as backbone.

![inspectorGenerator](https://github.com/Muciojad/CodeSamples/assets/10184394/e519edb4-b5f3-4a85-a2e3-9b89d639857d)


## Postman

Postman is my implementation of the publisher-subscriber code pattern. Messages are dispatched through the listeners quickly, using a Dictionary as the main collection for receivers. See "UseSamples" for real-life examples of using Postman in projects.

## Storage

A simple pure data-based storage system barebones for early-stage prototypes. It serves as basic player storage for collected/purchased items during the game. Easily extendable for save/load purposes and more complex storage operations.

# My custom dependency injection hobby project
With my experience in using Zenject/Extenject and some less complex DI systems, I decided to implement my own DI framework to fit the needs of my Poseidon Framework, which I use in my projects. So, I made it - it's called Poseidon Inseminator. It's an add-on for my Poseidon Framework, but you can take a look at the docs and the code to see what's going on there:
[GitHub Documentation & Code](https://github.com/Poseidon-tools/Poseidon-Inseminator)
