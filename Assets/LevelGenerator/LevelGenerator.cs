using System.Collections.Generic;
using UnityEngine;
using static Exit;
using static Section;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    Section[] sections;

    [SerializeField]
    List<SectionRule> sectionRules = new List<SectionRule>();

    [System.Serializable]
    public struct SectionRule
    {
        public SectionType sectionType;
        public SectionType[] allowSectionTransition;

        public int minCount;
        public int maxCount;

        public SectionType GetRandomAllowedSectionType()
        {
            if(allowSectionTransition.Length > 0)
                return allowSectionTransition[Random.Range(0, allowSectionTransition.Length)];
            
            return SectionType.CommonRoom;
        }
    }

    // Set of current generated sections
    HashSet<Section> currentSections = new HashSet<Section>();

    public bool IsGenerated { get; private set; } = false;

    [SerializeField]
    int seed = 0;
    [SerializeField]
    bool isCustomSeed = false;

    public bool disableRoomsOnStart = true;

    private void OnValidate()
    {
        List<SectionRule> copySectionRules = new List<SectionRule>();
        for (int i = 0; i < sectionRules.Count; i++)
        {
            SectionRule sectionRule = sectionRules[i];
            sectionRule.minCount = Mathf.Abs(sectionRule.minCount);
            sectionRule.maxCount = Mathf.Abs(sectionRule.maxCount);

            sectionRule.maxCount = Mathf.Clamp(sectionRule.maxCount, sectionRule.minCount, int.MaxValue);

            copySectionRules.Add(sectionRule);
        }

        sectionRules = copySectionRules;
    }

    private void Start()
    {
        // Set seed by GUID (to be checked if GUID is enough)
        if (!isCustomSeed)
        {
            seed = System.BitConverter.ToInt32(System.Guid.NewGuid().ToByteArray(), 0);
        }
        Random.InitState(seed);

        // Start generate level
        if (TryGetNextSectionOfType(SectionType.SpawnRoom, out Section section))
            Generate(InstantiateSection(section, Vector3.zero));
    }

    /// <summary>
    /// Main recursive generate function
    /// </summary>
    /// <param name="fromSection"></param>
    void Generate(Section fromSection)
    {
        // Get section rule from current section type
        SectionRule sectionRule = sectionRules.Find(x => x.sectionType == fromSection._SectionType);

        // List of free exits of section from we generate
        var freeExits = fromSection.GetFreeExits();

        // Go thow all free exits and see witch section we can generate
        for (int i = 0; i < freeExits.Count; i++)
        {
            // Additional check, if we generate all what we need
            if (IsAllRoomsGenerated())
            {
                InitializeSections();
                return;
            }

            // Get random section type from rule
            var sectionType = sectionRule.GetRandomAllowedSectionType();

            // Check if we can generate one more section of selected type
            if (!CanSpawnThisTypeOfRoom(sectionType))
                break;

            // Generate new section of selected type and connect it to target section
            if (TryGetNextSectionOfType(sectionType, out Section nextSection))
            {
                var targetExit = freeExits[Random.Range(0, freeExits.Count)];
                Vector3 targetPosition = fromSection.transform.position + GetOffsetByExitType(targetExit.m_ExitType);

                if (!CanGenerateHere(targetPosition))
                    continue;

                var section = InstantiateSection(nextSection, targetPosition);

                fromSection.SetConnection(section, targetExit);

                freeExits.Remove(targetExit);
            }
        }

        // Is we generate all that we need?
        if (!IsAllRoomsGenerated())
        {
            var from = new List<Section>(currentSections)[Random.Range(0, currentSections.Count)];
            Generate(from);

            return;
        }

        InitializeSections();
    }

    /// <summary>
    /// Final init section setup when we generate all
    /// </summary>
    void InitializeSections()
    {
        foreach (var section in currentSections)
        {
            section.Init();
        }
    }

    /// <summary>
    /// Check what we generate all what we need
    /// </summary>
    /// <returns></returns>
    bool IsAllRoomsGenerated()
    {
        foreach (var sectionRule in sectionRules)
        {
            List<Section> list = new List<Section>();
            foreach (var section in currentSections)
            {
                if (section._SectionType == sectionRule.sectionType)
                    list.Add(section);
            }
            int targetCount = Random.Range(sectionRule.minCount, sectionRule.maxCount + 1);

            if (list.Count != targetCount)
                return false;
        }

        IsGenerated = true;
        return true;
    }

    /// <summary>
    /// Check if we can spawn room of this type
    /// </summary>
    /// <param name="sectionType"></param>
    /// <returns></returns>
    bool CanSpawnThisTypeOfRoom(SectionType sectionType)
    {
        SectionRule sectionRule = sectionRules.Find(x => x.sectionType == sectionType);

        int currentCount = 0;
        foreach (var section in currentSections)
        {
            if (section._SectionType == sectionRule.sectionType)
                currentCount++;
        }

        return currentCount < sectionRule.maxCount;
    }

    /// <summary>
    /// Check by bound if we can generate there (need for right rooms setup, and maybe can do this better)))
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    bool CanGenerateHere(Vector3 targetPosition)
    {
        foreach (var section in currentSections)
        {
            if(section.transform.position == targetPosition)
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// To be changed (probably), because now it used olny for proper map generate
    /// </summary>
    /// <param name="exitType"></param>
    /// <returns></returns>
    Vector3 GetOffsetByExitType(ExitType exitType)
    {
        return exitType switch
        {
            // For now its just hardcoded offset
            //TODO: get offset from room bounds
            ExitType.Down => Vector3.back * 27,
            ExitType.Top => Vector3.forward * 27,
            ExitType.Left => Vector3.left * 27,
            ExitType.Right => Vector3.right * 27,
            _ => Vector3.zero,
        };
    }

    /// <summary>
    /// Returns random section of target type
    /// </summary>
    /// <param name="sectionType"></param>
    /// <param name="section"></param>
    /// <returns></returns>
    bool TryGetNextSectionOfType(SectionType sectionType, out Section section)
    {
        List<Section> sectionsList = new List<Section>(sections);
        sectionsList = sectionsList.FindAll(x => x._SectionType == sectionType);

        if(sectionsList.Count > 0)
        {
            section = sectionsList[Random.Range(0, sectionsList.Count)];
            return true;
        }

        section = null;
        return false;
    }

    /// <summary>
    /// Instantiate and Init section
    /// </summary>
    /// <param name="sectionPrefab"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    Section InstantiateSection(Section sectionPrefab, Vector3 position)
    {
        Section section = Instantiate(sectionPrefab, transform);
        section.gameObject.name += Random.Range(0, 100);
        section.transform.position = Vector3.zero;

        currentSections.Add(section);

        section.transform.position = position;

        // Can be changed, because I need it to be disabled))
        if (section._SectionType != SectionType.SpawnRoom)
            section.gameObject.SetActive(!disableRoomsOnStart);

        return section;
    }
}
