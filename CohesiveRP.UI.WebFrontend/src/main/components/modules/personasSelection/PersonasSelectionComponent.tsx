import styles from "./PersonasSelectionComponent.module.css";
import { useRef, useEffect, useState, useMemo } from "react";
import { FaFilter } from "react-icons/fa";
import { MdAddBox } from "react-icons/md";
import { AiOutlineDisconnect } from "react-icons/ai";
import { HiUserCircle } from "react-icons/hi";
import { MdChevronLeft, MdChevronRight, MdFirstPage, MdLastPage } from "react-icons/md";

/* Store */
import { sharedContext } from "../../../../store/AppSharedStoreContext";
import { getFromServerApiAsync, postToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { PersonaResponseDto } from "../../../../ResponsesDto/personas/PersonaResponseDto";
import type { PersonasResponseDto } from "../../../../ResponsesDto/personas/PersonasResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { SharedContextPersonaType } from "../../../../store/SharedContextPersonaType";
import { GetAvatarPathFromPersonaId, GetFallbackEmpty } from "../../../../utils/avatarUtils";
import { ImSpinner2 } from "react-icons/im";

type SortOption = "createdNewest" | "createdOldest" | "nameAZ" | "nameZA";

const PAGE_SIZE = 20;

const safeTime = (d?: string | null): number => {
  if (!d) return 0;
  const t = new Date(d).getTime();
  return isNaN(t) ? 0 : t;
};

export default function PersonasSelectionComponent() {
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);

  const [isLoadingPersonas, setIsLoadingPersonas] = useState(true);
  const [isAddingPersona,   setIsAddingPersona]   = useState(false);
  const [isNetworkDown,     setIsNetworkDown]      = useState(false);
  const [personasResponse,  setPersonasResponse]   = useState<PersonasResponseDto | null>(null);

  /* ── Filter / sort state ── */
  const [isFilterOpen,       setIsFilterOpen]       = useState(false);
  const [nameFilter,         setNameFilter]         = useState("");
  const [descriptionFilter,  setDescriptionFilter]  = useState("");
  const [sortOption,         setSortOption]         = useState<SortOption>("createdNewest");

  /* ── Pagination state ── */
  const [currentPage, setCurrentPage] = useState(1);

  /* ── Fetch ── */
  useEffect(() => {
    if (didComponentMountAlready.current) return;
    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        setIsLoadingPersonas(true);
        const response: PersonasResponseDto | null =
          await getFromServerApiAsync<PersonasResponseDto>("api/personas");

        const serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code !== 200 || serverApiException?.message) {
          console.error(`Call to fetch personas list failed. [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setPersonasResponse({ code: -1, personas: [] });
          return;
        }

        console.log("Personas list fetched successfully.");
        setPersonasResponse(response);
      } catch (error) {
        console.error("Fetch personas list error:", error);
      } finally {
        setIsLoadingPersonas(false);
      }
    };

    fetchData();
  }, []);

  /* ── Handlers ── */
  const handleSpecificPersonaClick = (personaId: string) => {
    navigateTo({
      selectedPersonaId: personaId,
      moduleName: "personaDetails",
    } as SharedContextPersonaType);
    console.log("PersonaDetails selected -> Module personaDetails selected.");
  };

  const handleAddPersonaClick = async () => {
    try {
      setIsAddingPersona(true);
      const response = await postToServerApiAsync<PersonaResponseDto>("api/personas", {});

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code !== 200 || serverApiException?.message) {
        console.error(
          `Create new persona failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}].`
        );
        return;
      }

      console.log("Persona created successfully.");
      const personaToAdd = response.persona;
      if (!personaToAdd) return;

      setPersonasResponse((prev) => {
        if (!prev) return { code: 200, personas: [personaToAdd] } as PersonasResponseDto;
        return { ...prev, personas: [personaToAdd, ...(prev.personas || [])] };
      });
      setCurrentPage(1);
    } catch (err) {
      console.error(err);
    } finally {
      setIsAddingPersona(false);
    }
  };

  /* ── Derived list ── */
  const activeFilterCount = [nameFilter, descriptionFilter].filter(Boolean).length;

  const filteredAndSorted = useMemo(() => {
    let list = [...(personasResponse?.personas ?? [])];

    if (nameFilter.trim()) {
      const q = nameFilter.trim().toLowerCase();
      list = list.filter((p) => p.name?.toLowerCase().includes(q));
    }
    if (descriptionFilter.trim()) {
      const q = descriptionFilter.trim().toLowerCase();
      list = list.filter((p) => p.description?.toLowerCase().includes(q));
    }

    switch (sortOption) {
      case "nameAZ":
        list.sort((a, b) => (a.name ?? "").localeCompare(b.name ?? ""));
        break;
      case "nameZA":
        list.sort((a, b) => (b.name ?? "").localeCompare(a.name ?? ""));
        break;
      case "createdOldest":
        list.sort((a, b) => safeTime(a.createdAtUtc) - safeTime(b.createdAtUtc));
        break;
      case "createdNewest":
      default:
        list.sort((a, b) => safeTime(b.createdAtUtc) - safeTime(a.createdAtUtc));
        break;
    }

    return list;
  }, [personasResponse?.personas, nameFilter, descriptionFilter, sortOption]);

  /* ── Reset to page 1 when filters change ── */
  const prevFilterKeyRef = useRef("");
  const filterKey = `${nameFilter}|${descriptionFilter}|${sortOption}`;
  if (prevFilterKeyRef.current !== filterKey) {
    prevFilterKeyRef.current = filterKey;
    if (currentPage !== 1) setCurrentPage(1);
  }

  /* ── Pagination derived values ── */
  const totalPages   = Math.max(1, Math.ceil(filteredAndSorted.length / PAGE_SIZE));
  const safePage     = Math.min(currentPage, totalPages);
  const pageStart    = (safePage - 1) * PAGE_SIZE;
  const currentItems = filteredAndSorted.slice(pageStart, pageStart + PAGE_SIZE);

  const pageButtons = useMemo(() => {
    if (totalPages <= 5) return Array.from({ length: totalPages }, (_, i) => i + 1);
    if (safePage <= 3)   return [1, 2, 3, 4, 5];
    if (safePage >= totalPages - 2)
      return [totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
    return [safePage - 2, safePage - 1, safePage, safePage + 1, safePage + 2];
  }, [totalPages, safePage]);

  /* ── Render ── */
  return (
    <main className={styles.personasComponent}>
      {isNetworkDown ? (
        <div className={styles.networkDownContainer}>
          <AiOutlineDisconnect className={styles.networkDownIcon} />
          <label>CohesiveRP backend is unreachable</label>
        </div>
      ) : isLoadingPersonas ? (
        <ImSpinner2 className={styles.loadingPersonasSpinner} />
      ) : (
        <div className={styles.personasMainContainer}>

          {/* ── Header bar ── */}
          <div className={styles.personasHeader}>
            <button
              className={`${styles.filterToggleButton} ${isFilterOpen ? styles.filterToggleActive : ""}`}
              onClick={() => setIsFilterOpen((prev) => !prev)}
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
              onChange={(e) => setSortOption(e.target.value as SortOption)}
            >
              <option value="createdNewest">Newest first</option>
              <option value="createdOldest">Oldest first</option>
              <option value="nameAZ">Name (A → Z)</option>
              <option value="nameZA">Name (Z → A)</option>
            </select>

            <div className={styles.personasToolsComponent}>
              {isAddingPersona ? (
                <ImSpinner2 className={styles.addingPersonaSpinner} />
              ) : (
                <MdAddBox
                  className={styles.addNewPersonaIcon}
                  onClick={handleAddPersonaClick}
                />
              )}
            </div>
          </div>

          {/* ── Collapsible filter panel ── */}
          <div className={`${styles.filterPanel} ${isFilterOpen ? styles.filterPanelOpen : ""}`}>
            <div className={styles.filterInner}>
              <div className={styles.filterField}>
                <label className={styles.filterLabel}>Name</label>
                <input
                  className={styles.filterInput}
                  type="text"
                  placeholder="Filter by name…"
                  value={nameFilter}
                  onChange={(e) => setNameFilter(e.target.value)}
                />
              </div>
              <div className={styles.filterField}>
                <label className={styles.filterLabel}>Description</label>
                <input
                  className={styles.filterInput}
                  type="text"
                  placeholder="Filter by description…"
                  value={descriptionFilter}
                  onChange={(e) => setDescriptionFilter(e.target.value)}
                />
              </div>
              {activeFilterCount > 0 && (
                <button
                  className={styles.clearButton}
                  onClick={() => { setNameFilter(""); setDescriptionFilter(""); }}
                >
                  Clear
                </button>
              )}
            </div>
            <div className={styles.resultsBar}>
              {filteredAndSorted.length} / {personasResponse?.personas?.length ?? 0} personas
            </div>
          </div>

          {/* ── Persona list ── */}
          <div className={styles.personasGridContainer}>
            {currentItems.length === 0 ? (
              <div className={styles.emptyState}>
                No personas match the current filters.
              </div>
            ) : (
              currentItems.map((persona) => (
                <div
                  key={persona.personaId}
                  className={persona.isDefault ? styles.personaContainerDefault : styles.personaContainer}
                  onClick={() => handleSpecificPersonaClick(persona.personaId)}
                >
                  <div className={styles.personaAvatarContainer}>
                    {persona.personaId ? (
                      <img
                        src={GetAvatarPathFromPersonaId(persona.personaId)}
                        alt={persona.name}
                        onError={(e) => {
                          e.currentTarget.onerror = null;
                          e.currentTarget.src = GetFallbackEmpty();
                        }}
                      />
                    ) : (
                      <HiUserCircle className={styles.personaAvatarFallback} />
                    )}
                  </div>

                  <div className={styles.personaInfoPanel}>
                    <div className={styles.personaNameRow}>
                      <label className={styles.personaNameLabel}>{persona.name}</label>
                      {persona.isDefault && (
                        <span className={styles.personaDefaultBadge}>Default</span>
                      )}
                    </div>
                    <label className={styles.personaDescriptionLabel}>
                      {(persona.description?.length ?? 0) > 1024
                        ? `${persona.description?.substring(0, 1024) ?? ""}…`
                        : persona.description}
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
                onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                disabled={safePage === 1}
                title="Previous page"
              >
                <MdChevronLeft />
              </button>

              {pageButtons[0] > 1 && (
                <span className={styles.paginationEllipsis}>…</span>
              )}

              {pageButtons.map((n) => (
                <button
                  key={n}
                  className={`${styles.paginationBtn} ${n === safePage ? styles.paginationBtnActive : ""}`}
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
                onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
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
                {pageStart + 1}–{Math.min(pageStart + PAGE_SIZE, filteredAndSorted.length)} of{" "}
                {filteredAndSorted.length}
              </span>
            </div>
          )}

        </div>
      )}
    </main>
  );
}
