using System;
using System.Collections.Generic;

namespace HustleThrough.Data
{
    [Serializable]
    public class RankDefinition
    {
        public int rank;
        public string title;
        public int unlockLevel;
        public string tier;
    }

    [Serializable]
    public class RankFile
    {
        public List<RankDefinition> ranks;
    }

    [Serializable]
    public class StoryFlagEntry
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class JobDefinition
    {
        public string id;
        public int level;
        public int rankRequired;
        public string title;
        public string location;
        public string taskDescription;
        public int cashReward;
        public bool isStoryGate;
        public bool isBonusPool;
        public string releaseBatch; // "launch" or "reserve" — reserve content ships in the
                                     // data file but is filtered out until you flip it live
        public bool isPlaceholder; // true for auto-generated content awaiting a narrative pass
        // Unity's JsonUtility can't deserialize a raw JSON object (storyFlagsSet)
        // into a Dictionary. Kept simple/omitted for v1 — see GameDataLoader for
        // the note on upgrading to a proper JSON library if flags need parsing.
    }

    [Serializable]
    public class JobFile
    {
        public List<JobDefinition> jobs;
    }
}
