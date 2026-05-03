import styles from "./CharactersSelectionComponent.module.css";
import { useRef, useEffect, useState, useMemo } from "react";
import { FaFilter } from "react-icons/fa";
import { MdAddBox } from "react-icons/md";
import { AiOutlineDisconnect } from "react-icons/ai";
import { MdChevronLeft, MdChevronRight, MdFirstPage, MdLastPage } from "react-icons/md";

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

const PAGE_SIZE = 25;

/* Safe UTC parser — avoids NaN comparisons when createdAtUtc is null/undefined/empty */
const safeTime = (d?: string | null): number => {
  if (!d) return 0;
  const t = new Date(d).getTime();
  return isNaN(t) ? 0 : t;
};

export default function CharactersSelectionComponent() {
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const newCharacterFileInputRef = useRef<HTMLInputElement | null>(null);

  const [isLoadingCharacters, setIsLoadingCharacters]   = useState(true);
  const [isImportingCharacter, setIsImportingCharacter] = useState(false);
  const [isNetworkDown, setIsNetworkDown]               = useState(false);
  const [charactersResponse, setCharactersResponse]     = useState<CharactersResponseDto | null>(null);

  /* ── Filter / sort state ── */
  const [isFilterOpen, setIsFilterOpen]           = useState(false);
  const [nameFilter, setNameFilter]               = useState('');
  const [descriptionFilter, setDescriptionFilter] = useState('');
  const [tagsFilter, setTagsFilter]               = useState('');
  const [sortOption, setSortOption]               = useState<SortOption>('createdNewest');

  /* ── Pagination state ── */
  const [currentPage, setCurrentPage] = useState(1);

  /* ── Fetch ── */
  useEffect(() => {
    if (didComponentMountAlready.current) return;
    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        setIsLoadingCharacters(true);
        const response: CharactersResponseDto | null =
          await getFromServerApiAsync<CharactersResponseDto>(`api/characters`);
        const serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Fetch characters list failed. [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setCharactersResponse({ code: -1, characters: [] });
          return;
        }
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
    navigateTo({
      selectedCharacterId: characterId,
      moduleName: "characterDetails",
    } as SharedContextCharacterType);
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
      const newCharacter = response as CharacterResponseDto;
      setCharactersResponse((prev) => {
        const charToAdd = newCharacter.character;
        if (!charToAdd) return prev;
        if (!prev) return { code: 200, characters: [charToAdd] } as CharactersResponseDto;
        return { ...prev, characters: [charToAdd, ...(prev.characters || [])] };
      });
      /* New character is prepended — jump to page 1 to show it */
      setCurrentPage(1);
    } catch (err) {
      console.error(err);
    } finally {
      event.target.value = "";
      setIsImportingCharacter(false);
    }
  };

  /* ── Derived list ── */
  const activeFilterCount = [nameFilter, descriptionFilter, tagsFilter].filter(Boolean).length;

  const filteredAndSorted = useMemo(() => {
    let list = [...(charactersResponse?.characters ?? [])];

    if (nameFilter.trim()) {
      const q = nameFilter.trim().toLowerCase();
      list = list.filter(c => c.name?.toLowerCase().includes(q));
    }
    if (descriptionFilter.trim()) {
      const q = descriptionFilter.trim().toLowerCase();
      list = list.filter(c =>
        c.description?.toLowerCase().includes(q) ||
        c.creatorNotes?.toLowerCase().includes(q)
      );
    }
    if (tagsFilter.trim()) {
      /* Support space- or comma-separated tokens — every token must match at least one tag */
      const tokens = tagsFilter.trim().toLowerCase().split(/[\s,]+/).filter(Boolean);
      list = list.filter(c => {
        const charTags = (c.tags ?? []).map(t => t.toLowerCase());
        return tokens.every(token => charTags.some(t => t.includes(token)));
      });
    }

    switch (sortOption) {
      case 'nameAZ':
        list.sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));
        break;
      case 'nameZA':
        list.sort((a, b) => (b.name ?? '').localeCompare(a.name ?? ''));
        break;
      case 'createdOldest':
        list.sort((a, b) => safeTime(a.createdAtUtc) - safeTime(b.createdAtUtc));
        break;
      case 'createdNewest':
      default:
        list.sort((a, b) => safeTime(b.createdAtUtc) - safeTime(a.createdAtUtc));
        break;
    }

    return list;
  }, [charactersResponse?.characters, nameFilter, descriptionFilter, tagsFilter, sortOption]);

  /* ── Reset to page 1 whenever the filtered/sorted list changes ── */
  const prevFilterKeyRef = useRef('');
  const filterKey = `${nameFilter}|${descriptionFilter}|${tagsFilter}|${sortOption}`;
  if (prevFilterKeyRef.current !== filterKey) {
    prevFilterKeyRef.current = filterKey;
    if (currentPage !== 1) setCurrentPage(1);
  }

  /* ── Pagination derived values ── */
  const totalPages   = Math.max(1, Math.ceil(filteredAndSorted.length / PAGE_SIZE));
  const safePage     = Math.min(currentPage, totalPages);
  const pageStart    = (safePage - 1) * PAGE_SIZE;
  const currentItems = filteredAndSorted.slice(pageStart, pageStart + PAGE_SIZE);

  /* ── Page range for compact button strip (max 5 buttons) ── */
  const pageButtons = useMemo(() => {
    if (totalPages <= 5) return Array.from({ length: totalPages }, (_, i) => i + 1);
    if (safePage <= 3)   return [1, 2, 3, 4, 5];
    if (safePage >= totalPages - 2) return [totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
    return [safePage - 2, safePage - 1, safePage, safePage + 1, safePage + 2];
  }, [totalPages, safePage]);

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
          {/* ── Header bar ── */}
          <div className={styles.charactersHeader}>
            <button
              className={`${styles.filterToggleButton} ${isFilterOpen ? styles.filterToggleActive : ''}`}
              onClick={() => setIsFilterOpen(prev => !prev)}
              title="Toggle filters"
            >
              <FaFilter />
              {activeFilterCount > 0 && (
                <span className={styles.filterBadge}>{activeFilterCount}</span>
              )}
            </button>

            <select
              className={styles.sortSelect}
              value={sortOption}
              onChange={e => setSortOption(e.target.value as SortOption)}
            >
              <option value="createdNewest">Newest first</option>
              <option value="createdOldest">Oldest first</option>
              <option value="nameAZ">Name (A → Z)</option>
              <option value="nameZA">Name (Z → A)</option>
            </select>

            <div className={styles.charactersToolsComponent}>
              {isImportingCharacter ? (
                <ImSpinner2 className={styles.importingCharacterSpinner} />
              ) : (
                <>
                  <MdAddBox
                    className={styles.addNewCharacterIcon}
                    onClick={() => newCharacterFileInputRef.current?.click()}
                  />
                  <input
                    type="file"
                    ref={newCharacterFileInputRef}
                    style={{ display: "none" }}
                    onChange={handleAddCharacterFileSelected}
                  />
                </>
              )}
            </div>
          </div>

          {/* ── Collapsible filter panel ── */}
          <div className={`${styles.filterPanel} ${isFilterOpen ? styles.filterPanelOpen : ''}`}>
            <div className={styles.filterInner}>
              <div className={styles.filterField}>
                <label className={styles.filterLabel}>Title</label>
                <input
                  className={styles.filterInput}
                  type="text"
                  placeholder="Filter by name…"
                  value={nameFilter}
                  onChange={e => setNameFilter(e.target.value)}
                />
              </div>
              <div className={styles.filterField}>
                <label className={styles.filterLabel}>Description</label>
                <input
                  className={styles.filterInput}
                  type="text"
                  placeholder="Filter by description or notes…"
                  value={descriptionFilter}
                  onChange={e => setDescriptionFilter(e.target.value)}
                />
              </div>
              <div className={styles.filterField}>
                <label className={styles.filterLabel}>Tags</label>
                <input
                  className={styles.filterInput}
                  type="text"
                  placeholder="e.g. fantasy, elf…"
                  value={tagsFilter}
                  onChange={e => setTagsFilter(e.target.value)}
                />
              </div>
              {activeFilterCount > 0 && (
                <button
                  className={styles.clearButton}
                  onClick={() => { setNameFilter(''); setDescriptionFilter(''); setTagsFilter(''); }}
                >
                  Clear
                </button>
              )}
            </div>
            <div className={styles.resultsBar}>
              {filteredAndSorted.length} / {charactersResponse?.characters?.length ?? 0} characters
            </div>
          </div>

          {/* ── Character list ── */}
          <div className={styles.charactersGridContainer}>
            {currentItems.length === 0 ? (
              <div className={styles.emptyState}>
                No characters match the current filters.
              </div>
            ) : (
              currentItems.map(character => (
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
                      {character.tags?.join(" / ") ?? ""}
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

          {/* ── Pagination bar ── */}
          {totalPages > 1 && (
            <div className={styles.paginationBar}>
              <button
                className={styles.paginationBtn}
                onClick={() => setCurrentPage(1)}
                disabled={safePage === 1}
                title="First page"
              >
                <MdFirstPage />
              </button>
              <button
                className={styles.paginationBtn}
                onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                disabled={safePage === 1}
                title="Previous page"
              >
                <MdChevronLeft />
              </button>

              {pageButtons[0] > 1 && (
                <span className={styles.paginationEllipsis}>…</span>
              )}

              {pageButtons.map(n => (
                <button
                  key={n}
                  className={`${styles.paginationBtn} ${n === safePage ? styles.paginationBtnActive : ''}`}
                  onClick={() => setCurrentPage(n)}
                >
                  {n}
                </button>
              ))}

              {pageButtons[pageButtons.length - 1] < totalPages && (
                <span className={styles.paginationEllipsis}>…</span>
              )}

              <button
                className={styles.paginationBtn}
                onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                disabled={safePage === totalPages}
                title="Next page"
              >
                <MdChevronRight />
              </button>
              <button
                className={styles.paginationBtn}
                onClick={() => setCurrentPage(totalPages)}
                disabled={safePage === totalPages}
                title="Last page"
              >
                <MdLastPage />
              </button>

              <span className={styles.paginationInfo}>
                {pageStart + 1}–{Math.min(pageStart + PAGE_SIZE, filteredAndSorted.length)} of {filteredAndSorted.length}
              </span>
            </div>
          )}
        </div>
      )}
    </main>
  );
}
