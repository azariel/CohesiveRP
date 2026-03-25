namespace CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects
{
    public enum PathfinderAttributes
    {
        Fortitude = 0,
        Reflex = 1,
        Willpower = 2,// Counter attribue to the Charisma, Intimidation skills.
        Stamina = 3,// endurance
        MagicalStamina = 4,// Depending on the story context, if magic exists, we want to limit the character 'mana'
        Intelligence = 5,
        Discernment = 6,// Counter attribute to the Deception skill. Discernment is the ability to clearly judge what is true, real, or valuable. When a character is being lied to
        Perception = 7,// Counter attribute to the Stealth skill
    }
}
