# CLAUDE.md
This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository. See @package.json for available npm commands and project dependencies.

# What this project is
We are working together on a project made using Vite, React, Typescript. This WebApp uses HttpRequest to communicate with a backend made in C#. The application is named 'CohesiveRp', it's an application to allow roleplay between a User and an AI (LLMs).

# Settings
Note that 'erasableSyntaxOnly' is enabled.

# UI
Dark Arcane Editorial theme (Dark cyan)

## Backend
The backend is made using c# 10. It exposes many routes. When we receive a new message from the User (typically from the UI), the backend queue two background tasks.

One to update the sceneTracker(sceneDirector), which tracks the main themes of the scene (such as Romance, Combat, etc), nestedThemes, currentDateTime (in the roleplay), location, characters (name, mood, facialExpression, outfit, underwear, stateOfDress(partially dressed, fully dressed), exposedBodyParts, clothingStateOfDress, hairStyle, posture, semenOnBodyLocation, bodyPosition, personalGainInScene, relevantKinksInScene, relevantSecretKinksInScene, relevantPersonalityTraits, innerThoughtsOrMonologue, nextActionsAccordingToPersonality).

The second background task is to update the SkillCheckInitiator, which will analyse the scene and infer what Attributes or Skills may be in play. The backend will then roll dices, using the characterSheets attributes and skills to infer their success or failures in the scene.

The third background task is to give a narrative direction. The role of this step is to find out and guide what should happen next.

After those three tasks, the main task is queued. The main task will query the main LLM with the context, characters in scene, rolls, etc.

After this main task, the backend will queue summarization background tasks to summarize the messages between the User and AI and to summarize the summaries to shrink the space required in the context.

The backend will also queue a prose guardian to enforce the next AI reply prose. It analyzes the last messages and produce concrete, actionable writing directives for the next generation.

### Character Sheet
A character sheet represents the information about a character that helps the AI impersonate it. The information include first name, last name, birthday, age group, age group appearance (when the character looks younger than their age for instance), gender, sexuality (hetero, bisexual, etc), race/species, profession, body type, height, eye color, skin color, hair color, hair style, ear shape, genitals, breasts size, penis size, attractiveness, clothes preference, speech pattern, speedch impairment, mannerisms, social anxiety, behavior(personality), personality traits, likes, dislikes, fears, secrets, skills, weaknesses, reputation, relationships, preferred combat style, weapons proficiency, combat affinity (attack), combat affinity (defense), goals for the next year, long-term goals, kinks, secret kinks, pathfinder attributes (sort of DnD attributes), pathfinder skills (sort of DnD skills).

#### Character Sheet Instance
When a new chat is created, a copy of the Character Sheet of each character linked to that Chat is created. As long as the data within the instance is identical to the parent (Character Sheet), any modification on the character sheet will be reflected to the character sheet instance. Otherwise, it becomes distinct. This allow a character within a specific Chat to change, evolve, grow over the course of the story and roleplay without impacting the initial 'blueprint'.

## Frontend
The frontend gives a way for the user to interact with the AI, by roleplaying (Chat tab), but also to configure what characters are available, how they are setup, the images, the inference servers available, the completion presets, etc.

### Header
The header allows the user of the application to select a View to show on screen. The Header is always anchored at the very top of the screen and takes 100% width, positioning the buttons in a reactive way to adapt to different screen resolutions.

#### Chat
The first tab is a view to select a specific chat. A carrousel of Chats appear for the user to choose. Each chat is represented by the Chat avatar topped by the chat name. They are ordered by last activity first(most recent first). When the User clicks on a specific chat avatar, the specialized Chat Interactive view opens.

#### Characters
The second tab is a view of the characters. There is a button to add a new character, filter available characters, etc. When the User clicks on a character, the CharacterDetails view opens. In this view, the character name, description, tags, first message, creator, creator notes is shown. Those are editable. There is also an option to open any related Chat or to create a new Chat from this character. The sub-tab 'Info' is selected by default. When selecting the sub-tab 'Character Sheet', the User is shown a component with the character sheet of this character. A button to generate the fields from an LLM is available. All fields are editable. When selecting the sub-tab 'Illustration', the User is shown a component with the character illustration fields and settings (UI), such as illustrator tag, outfit and illustrator prompt injection as well as a carousel with the generated images acting as that character avatar.

#### Player (Personas)
The third tab is Personas. It's fairly similar to Characters, but those are reserved for User impersonation. When the user clicks on a Persona, the PersonaDetails view opens. In this view, contrary to the CharacterDetails, there is only the Description field available and thus editable. The sub-tab 'Info' is selected by default. When selecting the sub-tab 'Character Sheet', the User is shown a component with the character sheet of this character. A button to generate the fields from an LLM is available. All fields are editable. The component CharacterSheet is the same as the one for the characters.

#### Lorebooks
The fourth tab is Lorebooks. The view show a list of available Lorebooks as well as a button to add new ones. When the User click on a Lorebook, a view opens to allow the User to edit the lorebook title and every fields within that lorebook.

#### Completion Presets
The fifth tab is the CompletionPresets tab. It allows the User to customize the prompt and context that is used by the background on the various backgroundTasks, such as when summarizing the story, generating a scene tracker, defining the skill checks initiator, dynamic creation of characters, dynamic creation of character sheets, dynamic generation of character avatars for illustration(with comfyui) or for the main chat.

#### Settings
The sixth and last tab shows the settings related to the LLM Apis, completion presets to use (the fifth tab is the one to customize them), summaries handling.

### Sub Header
The sub header allows the user of the application to select a specialized View, that are *tied to a specific Chat* to show on screen. The SubHeader is always anchored right below the Header (at the top of the screen) and takes 100% width, positioning the buttons in a reactive way to adapt to different screen resolutions.

#### Chat Interactive
That view shows the latest messages (called Hot messages), the avatars of the characters currently in the scene, the input to allow the user to send new message, the scene tracker and some other buttons (to edit message, delete message, etc).

##### Messages
The component shows the Hot messages, which is a list of messages kept in the backend database. That list is dynamically reduced to remove the oldest message and store it as Cold message in the database to always keep a lean Hot messages array ready. the three latest messages are editable by double clicking on the message. Each messages show an embedded button 'Thinking' to show the AI thinking process when available. The header of the message also shows the character name (or the player persona's name), the date at which the message was generated. On the left side of each message, we show an avatar representing the main focus at that moment(inferred from the sceneTracker module), followed by the message index (iteration number) and finally how much time it took to generate the message. Right before the most recent AI message, we insert the mobileAvatarBanner, which is a module that shows the avatars of the characters tracked by the sceneTracker in the scene. Below, we show the sceneTracker and finally, the latest AI message. The latest message offers some unique buttons: ViewPrompt(show a modal box with text to show what the backend sends to the 'main' request), delete(to delete that message as well as all PRE operations such as the current sceneTracker, skillChecks, etc) and the Swipe button(Regenerate the main query, but keep all PRE-operations intact).

##### Scene Rolls
The component sits below the latest message from the AI and shows the latest Pathfinder rolls of each characters in the scene. It's collapsible.

##### Input
This component allow the user to write a message and send it, which will trigger the PRE-operations to generate and following that, the 'main' query to trigger so that the AI reply is also generated with the whole context. The whole context is built from the story context, characters, persona, memories as well as the PRE-operations results and a few other components. To the absolute right of this component, we show either a spinner to show that the AI is currently in the process of replying or we show an arrow to allow the user to send its message to the backend.

#### Character Sheet Instances