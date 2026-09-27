import { useCallback, useEffect, useRef, useState } from "react";
import { getFromServerApiAsync } from "../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { BackgroundQueriesResponseDto } from "../../ResponsesDto/chat/BackgroundQueriesResponseDto";
import type { BackgroundQuery } from "../../ResponsesDto/chat/BusinessObjects/BackgroundQuery";

const POLL_INTERVAL_MS = 5000;

const isActiveStatus = (status: string) => status === "Pending" || status === "InProgress";

/**
 * Generic poller for a chat's background queries. Polls every 5s for as long as at
 * least one query is Pending/InProgress, then stops. `triggerPoll` forces an
 * immediate fetch and (re)starts the interval if needed — used both for the
 * "poll once on load/refresh" case and the "start polling right after send" case.
 *
 * `ready` gates the very first fetch — pass false until it's safe to query
 * (e.g. until the initial hot-messages fetch has landed), to avoid racing other
 * effects that also mutate chat state on load.
 */
export function useBackgroundQueriesPoller(chatId: string | undefined, ready: boolean) {
  const [queries, setQueries] = useState<BackgroundQuery[]>([]);
  const [isLoadingInitial, setIsLoadingInitial] = useState(true);
  const [networkError, setNetworkError] = useState(false);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const chatIdRef = useRef(chatId);
  chatIdRef.current = chatId;

  const stopPolling = useCallback(() => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
  }, []);

  const fetchOnce = useCallback(async () => {
    const currentChatId = chatIdRef.current;
    if (!currentChatId)
      return;

    const response = await getFromServerApiAsync<BackgroundQueriesResponseDto>(
      `api/backgroundQueries?chatId=${currentChatId}`
    );

    const serverApiException = response as ServerApiExceptionResponseDto | null;
    if (!response || response.code !== 200 || serverApiException?.message) {
      console.error(`Fetching background queries failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}].`);
      setNetworkError(true);
      return;
    }

    setNetworkError(false);
    setIsLoadingInitial(false);

    const fetched = response.queries ?? [];
    setQueries(fetched);

    if (fetched.some((q) => isActiveStatus(q.status))) {
      if (!intervalRef.current) {
        intervalRef.current = setInterval(fetchOnce, POLL_INTERVAL_MS);
      }
    } else {
      stopPolling();
    }
  }, [stopPolling]);

  useEffect(() => {
    if (!chatId || !ready)
      return;

    fetchOnce();
    return () => stopPolling();
  }, [chatId, ready, fetchOnce, stopPolling]);

  const triggerPoll = useCallback(() => {
    fetchOnce();
  }, [fetchOnce]);

  return { queries, isLoadingInitial, networkError, triggerPoll };
}