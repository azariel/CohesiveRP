import styles from "./CharactersSelectionComponent.module.css";
import { useRef, useEffect, useState, useMemo } from "react";
import { FaFilter } from "react-icons/fa";
import { MdAddBox } from "react-icons/md";
import { AiOutlineDisconnect } from "react-icons/ai";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import { getFromServerApiAsync, postToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { CharacterResponseDto } from "../../../../ResponsesDto/characters/CharacterResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { CharactersResponseDto } from "../../../../ResponsesDto/characters/CharactersResponseDto";
import { GetAvatarPathFromCharacterName, GetFallbackEmpty } from "../../../../utils/avatarUtils";
import type { SharedContextCharacterType } from "../../../../store/SharedContextCharacterType";
import { ImSpinner2 } from "react-icons/im";

type SortOption = 'createdNewest' | 'createdOldest' | 'nameAZ' | 'nameZA';

export default function CharactersSelectionComponent() {
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const newCharacterFileInputRef = useRef<HTMLInputElement | null>(null);

  const [isLoadingCharacters, setIsLoadingCharacters] = useState(true);
  const [isImportingCharacter, setIsImportingCharacter] = useState(false);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [charactersResponse, setCharactersResponse] = useState<CharactersResponseDto | null>(null);

  /* ── Filter / sort state ── */
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [nameFilter, setNameFilter]             = useState('');
  const [creatorFilter, setCreatorFilter]       = useState('');
  const [descriptionFilter, setDescriptionFilter] = useState('');
  const [sortOption, setSortOption]             = useState<SortOption>('createdNewest');

  /* ── Fetch ── */
  useEffect(() => {
    if (didComponentMountAlready.current) return;
    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        setIsLoadingCharacters(true);
        const response: CharactersResponseDto | null = await getFromServerApiAsync<CharactersResponseDto>(`api/characters`);
        const serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch characters list failed. [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setCharactersResponse({ code: -1, characters: [] });
          return;
        }
        console.log(`Characters list fetched successfully.`);
        setCharactersResponse(response);
      } catch (error) {
        console.error("Fetch characters list error:", error);
      } finally {
        setIsLoadingCharacters(false);
      }
    };

    fetchData();
  }, []);

  /* ── Handlers ── */
  const handleSpecificCharacterClick = (characterId: string) => {
    const selectedCharacter = {
      selectedCharacterId: characterId,
      moduleName: "characterDetails",
    } as SharedContextCharacterType;
    navigateTo(selectedCharacter);
  };

  const handleAddCharacterClick = () => {
    newCharacterFileInputRef.current?.click();
  };

  const handleAddCharacterFileSelected = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const formData = new FormData();
    formData.append("file", file);

    try {
      setIsImportingCharacter(true);
      const response = await postToServerApiAsync<CharacterResponseDto>("api/characters", formData);
      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code != 200 || serverApiException?.message) {
        console.error(`Upload new character failed.`);
      }
      console.log(`Character uploaded successfully.`);
      const newCharacter = response as CharacterResponseDto;
      setCharactersResponse((prev) => {
        const charToAdd = newCharacter.character;
        if (!charToAdd) return prev;
        if (!prev) return { code: 200, characters: [charToAdd] } as CharactersResponseDto;
        return { ...prev, characters: [charToAdd, ...(prev.characters || [])] };
      });
    } catch (err) {
      console.error(err);
    } finally {
      event.target.value = "";
      setIsImportingCharacter(false);
    }
  };

  const handleClearFilters = () => {
    setNameFilter('');
    setCreatorFilter('');
    setDescriptionFilter('');
    setSortOption('createdNewest');
  };

  /* ── Derived list ── */
  const activeFilterCount = [nameFilter, creatorFilter, descriptionFilter].filter(Boolean).length;
  const hasNonDefaultSort = sortOption !== 'createdNewest';

  const filteredAndSorted = useMemo(() => {
    let list = [...(charactersResponse?.characters ?? [])];

    if (nameFilter.trim()) {
      const q = nameFilter.toLowerCase();
      list = list.filter(c => c.name?.toLowerCase().includes(q));
    }
    if (creatorFilter.trim()) {
      const q = creatorFilter.toLowerCase();
      list = list.filter(c => c.creator?.toLowerCase().includes(q));
    }
    if (descriptionFilter.trim()) {
      const q = descriptionFilter.toLowerCase();
      list = list.filter(c =>
        c.description?.toLowerCase().includes(q) ||
        c.creatorNotes?.toLowerCase().includes(q)
      );
    }

    switch (sortOption) {
      case 'nameAZ':
        list.sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));
        break;
      case 'nameZA':
        list.sort((a, b) => (b.name ?? '').localeCompare(a.name ?? ''));
        break;
      case 'createdOldest':
        list.sort((a, b) => new Date(a.createdAtUtc).getTime() - new Date(b.createdAtUtc).getTime());
        break;
      case 'createdNewest':
      default:
        list.sort((a, b) => new Date(b.createdAtUtc).getTime() - new Date(a.createdAtUtc).getTime());
        break;
    }

    return list;
  }, [charactersResponse?.characters, nameFilter, creatorFilter, descriptionFilter, sortOption]);

  /* ── Render ── */
  return (
    <main className={styles.charactersComponent}>
      {isNetworkDown ? (
        <div className={styles.networkDownContainer}>
          <AiOutlineDisconnect className={styles.networkDownIcon} />
          <label>CohesiveRP backend is unreachable</label>
        </div>
      ) : isLoadingCharacters ? (
        <ImSpinner2 className={styles.loadingCharactersSpinner} />
      ) : (
        <div className={styles.charactersMainContainer}>

          {/* ── Header ── */}
          <div className={styles.charactersHeader}>
            <button
              className={`${styles.filterToggleButton} ${isFilterOpen ? styles.filterToggleActive : ''}`}
              onClick={() => setIsFilterOpen(prev => !prev)}
              title="Toggle filters & sort"
            >
              <FaFilter className={styles.filterIcon} />
              {activeFilterCount > 0 && (
                <span className={styles.filterBadge}>{activeFilterCount}</span>
              )}
            </button>
            <div className={styles.charactersToolsComponent}>
              {isImportingCharacter ? (
                <ImSpinner2 className={styles.importingCharacterSpinner} />
              ) : (
                <div>
                  <MdAddBox className={styles.addNewCharacterIcon} onClick={handleAddCharacterClick} />
                  <input
                    type="file"
                    ref={newCharacterFileInputRef}
                    style={{ display: "none" }}
                    onChange={handleAddCharacterFileSelected}
                  />
                </div>
              )}
            </div>
          </div>

          {/* ── Filter panel ── */}
          <div className={`${styles.filterPanel} ${isFilterOpen ? styles.filterPanelOpen : ''}`}>
            <div className={styles.filterGrid}>
              <div className={styles.filterField}>
                <label className={styles.filterLabel}>Name</label>
                <input
                  className={styles.filterInput}
                  type="text"
                  placeholder="Search by name…"
                  value={nameFilter}
                  onChange={e => setNameFilter(e.target.value)}
                />
              </div>
              <div className={styles.filterField}>
                <label className={styles.filterLabel}>Creator</label>
                <input
                  className={styles.filterInput}
                  type="text"
                  placeholder="Search by creator…"
                  value={creatorFilter}
                  onChange={e => setCreatorFilter(e.target.value)}
                />
              </div>
              <div className={styles.filterField}>
                <label className={styles.filterLabel}>Sort by</label>
                <select
                  className={styles.filterSelect}
                  value={sortOption}
                  onChange={e => setSortOption(e.target.value as SortOption)}
                >
                  <option value="createdNewest">Newest first</option>
                  <option value="createdOldest">Oldest first</option>
                  <option value="nameAZ">Name (A → Z)</option>
                  <option value="nameZA">Name (Z → A)</option>
                </select>
              </div>
            </div>
            <div className={styles.filterDescRow}>
              <div className={styles.filterDescField}>
                <label className={styles.filterLabel}>Description / Notes</label>
                <input
                  className={styles.filterInput}
                  type="text"
                  placeholder="Search description or creator notes…"
                  value={descriptionFilter}
                  onChange={e => setDescriptionFilter(e.target.value)}
                />
              </div>
              {(activeFilterCount > 0 || hasNonDefaultSort) && (
                <button className={styles.filterClearButton} onClick={handleClearFilters}>
                  Clear
                </button>
              )}
            </div>
          </div>

          {/* ── Results info ── */}
          {isFilterOpen && (
            <div className={styles.resultsBar}>
              {filteredAndSorted.length} of {charactersResponse?.characters?.length ?? 0} characters
            </div>
          )}

          {/* ── Grid ── */}
          <div className={styles.charactersGridContainer}>
            {filteredAndSorted.length === 0 ? (
              <div className={styles.emptyState}>
                No characters match the current filters.
              </div>
            ) : (
              filteredAndSorted.map(character => (
                <div
                  key={character.characterId}
                  className={styles.characterContainer}
                  onClick={() => handleSpecificCharacterClick(character.characterId)}
                >
                  <div className={styles.characterAvatarContainer}>
                    <img
                      src={GetAvatarPathFromCharacterName(character.name)}
                      alt="Avatar"
                      onError={(e) => {
                        e.currentTarget.onerror = null;
                        e.currentTarget.src = GetFallbackEmpty();
                      }}
                    />
                  </div>
                  <div className={styles.characterInfoPanel}>
                    <label className={styles.characterCharNameLabel}>{character.name}</label>
                    <label className={styles.characterTagsLabel}>
                      {!character.tags ? "" : character.tags.join(" / ")}
                    </label>
                    <label className={styles.characterDescriptionLabel}>
                      {(character.creatorNotes?.length ?? 0) > 512
                        ? `${character.creatorNotes.substring(0, 512)}…`
                        : character.creatorNotes}
                    </label>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </main>
  );
}
