import { useCallback, useEffect, useState } from 'react';
import Header from '../components/Header';
import { auditApi } from '../api/client';
import type { AuditEntry, AuditMeta } from '../types/api';

function pretty(obj: unknown) {
  try { return JSON.stringify(obj, null, 2); } catch { return String(obj); }
}

// formatWhen removed — per-entry times formatted explicitly where needed

function actionBadge(action: string) {
  const a = action?.toLowerCase?.() ?? '';
  if (a.includes('create')) return 'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300';
  if (a.includes('delete')) return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300';
  if (a.includes('update')) return 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300';
  return 'bg-gray-100 text-gray-800 dark:bg-gray-800/30 dark:text-gray-300';
}

function shortId(s?: string) {
  if (!s) return '—';
  return s.length > 8 ? s.slice(0, 8) + '…' : s;
}

function describeEntry(entry: AuditEntry) {
  const nv = (entry.newValues ?? {}) as Record<string, any>;
  const ov = (entry.oldValues ?? {}) as Record<string, any>;
  if (entry.entity === 'Tasks') {
    const title = nv?.Title ?? ov?.Title ?? nv?.title ?? ov?.title;
    return title ? `Task: ${title}` : `Task (${shortId(entry.entityId)})`;
  }
  if (entry.entity === 'Users') {
    const uname = nv?.Username ?? ov?.Username ?? nv?.username ?? ov?.username;
    return uname ? `User: ${uname}` : `User (${shortId(entry.entityId)})`;
  }
  const label = nv?.name ?? ov?.name ?? nv?.title ?? ov?.title;
  return label ? `${entry.entity}: ${label}` : `${entry.entity} (${shortId(entry.entityId)})`;
}

function getChangedFields(oldV?: Record<string, any> | null, newV?: Record<string, any> | null) {
  const o = oldV ?? {};
  const n = newV ?? {};
  const keys = Array.from(new Set([...Object.keys(o), ...Object.keys(n)]));
  const changed: string[] = [];
  for (const k of keys) {
    try {
      const ov = o[k];
      const nv = n[k];
      if (JSON.stringify(ov) !== JSON.stringify(nv)) changed.push(k);
    } catch {
      // ignore serialization issues and conservatively mark as changed
      changed.push(k);
    }
  }
  return changed;
}

export default function AuditTasks() {
  const [entries, setEntries] = useState<AuditEntry[]>([]);
  const [serverMeta, setServerMeta] = useState<AuditMeta['server'] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [entityFilter, setEntityFilter] = useState<string>('All');
  const [actionFilter, setActionFilter] = useState<string>('All');
  const [query, setQuery] = useState('');
  const [expanded, setExpanded] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [entities, setEntities] = useState<string[]>([]);
  const [actions, setActions] = useState<string[]>([]);

  const load = useCallback(async () => {
    setLoading(true); setError(null);
    try {
      // fetch metadata (entities/actions) once
      const meta = await auditApi.meta();
      setEntities(['All', ...meta.entities]);
      setActions(['All', ...meta.actions]);
      setServerMeta(meta.server ?? null);

      const ent = entityFilter && entityFilter !== 'All' ? entityFilter : undefined;
      const act = actionFilter && actionFilter !== 'All' ? actionFilter : undefined;
      const res = await auditApi.list(page, pageSize, ent, act, query || undefined);
      setEntries(res.items);
      setTotalCount(res.totalCount);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load audit');
      // fallback sample data so UI can be demonstrated during interview
      const samples = [
        {
          id: 'sample-1', entity: 'Tasks', entityId: 'task-1', action: 'Update', userId: 'demo', username: 'demo',
          oldValues: { title: 'Initial title', status: 'Todo' },
          newValues: { title: 'Updated title', status: 'InProgress' },
          timestamp: new Date().toISOString(),
        },
        {
          id: 'sample-2', entity: 'Users', entityId: 'user-1', action: 'Create', userId: 'system', username: 'system',
          oldValues: null, newValues: { username: 'alice', email: 'alice@example.com', role: 'User' },
          timestamp: new Date(Date.now() - 1000 * 60 * 60).toISOString(),
        }
      ];
      setEntries(samples);
      setTotalCount(samples.length);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, entityFilter, actionFilter, query]);

  useEffect(() => { load(); }, [load]);

  useEffect(() => { setPage(1); }, [entityFilter, actionFilter, query, pageSize]);

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  useEffect(() => { if (page > totalPages) setPage(totalPages); }, [totalPages, page]);

  const showingStart = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const showingEnd = Math.min(totalCount, page * pageSize);

  function toggle(id: string) { setExpanded(e => e === id ? null : id); }

  async function copyText(text: string) {
    try { await navigator.clipboard.writeText(text); } catch {}
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-gray-100">
      <Header />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 flex flex-col">
        <div className="flex items-center justify-between mb-6 gap-4">
          <div>
            <h1 className="text-2xl font-bold">Audit Trail</h1>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">A chronological record of changes to `Users` and `Tasks`.</p>
          </div>
          <div className="flex items-center gap-2">
            <button onClick={load} className="px-3 py-2 bg-indigo-600 text-white rounded-xl text-sm">Refresh</button>
          </div>
        </div>

        <div className="flex-1" style={{ height: 'calc(100vh - 180px)' }}>
          <div className="grid grid-cols-1 lg:grid-cols-4 gap-6 h-full">
            <aside className="lg:col-span-1 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 p-4 space-y-3 lg:sticky lg:top-6 self-start">
            <label className="text-xs font-semibold text-gray-600 dark:text-gray-400">Entity</label>
            <select value={entityFilter} onChange={e => setEntityFilter(e.target.value)} className="w-full rounded-md p-2 border bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 border-gray-200 dark:border-gray-700">
              {(entities.length ? entities : ['All']).map(ent => <option key={ent} value={ent}>{ent}</option>)}
            </select>

            <label className="text-xs font-semibold text-gray-600 dark:text-gray-400">Action</label>
            <select value={actionFilter} onChange={e => setActionFilter(e.target.value)} className="w-full rounded-md p-2 border bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 border-gray-200 dark:border-gray-700">
              {(actions.length ? actions : ['All']).map(a => <option key={a} value={a}>{a}</option>)}
            </select>

            <label className="text-xs font-semibold text-gray-600 dark:text-gray-400">Search</label>
            <input value={query} onChange={e => setQuery(e.target.value)} placeholder="user, entity id or type" className="w-full rounded-md p-2 border text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 border-gray-200 dark:border-gray-700" />

            <div className="pt-2 text-xs text-gray-500">Showing <strong className="text-gray-700 dark:text-gray-200">{showingStart}-{showingEnd}</strong> of <strong className="text-gray-700 dark:text-gray-200">{totalCount}</strong> entries</div>
            {error && <div className="text-xs text-red-500">{error}</div>}
          </aside>

          <main className="lg:col-span-3 h-full flex flex-col">
            {loading ? (
              <div className="p-6 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700">Loading…</div>
            ) : totalCount === 0 ? (
              <div className="p-6 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 text-center">No audit entries found.</div>
            ) : (
              <>
                <div className="space-y-4 pr-2 flex-1 overflow-y-auto min-h-0">
                  {entries.map(entry => (
                    <div key={entry.id} className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 p-4">
                      <div className="flex items-start justify-between gap-4">
                        <div className="flex-1">
                          <div className="flex items-center gap-3">
                            <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${actionBadge(entry.action)}`}>{entry.action}</span>
                            <div className="text-sm font-medium">{entry.entity}</div>
                            <div className="text-xs text-gray-400">{describeEntry(entry)}</div>
                          </div>
                          <div className="mt-1 text-xs text-gray-400">
                            {(() => {
                              const changed = getChangedFields(entry.oldValues as any, entry.newValues as any);
                              if (!changed || changed.length === 0) return null;
                              if (changed.length <= 3) return <span>Changed: {changed.join(', ')}</span>;
                              return <span>Changed: {changed.length} fields ({changed.slice(0, 3).join(', ')}...)</span>;
                            })()}
                          </div>
                          <div className="mt-2 text-xs text-gray-500 flex items-center gap-3">
                            <div>By: <span className="font-medium text-gray-700 dark:text-gray-200">{entry.username ?? entry.userId ?? 'system'}</span></div>
                            <div>·</div>
                            <div className="flex items-center gap-3">
                              {(() => {
                                const iso = (entry as any).timestamp ?? (entry as any).createdAt ?? (entry as any).created_at;

                                function countryCodeToEmoji(code?: string) {
                                  if (!code) return '🌐';
                                  const cc = code.toUpperCase();
                                  if (cc.length !== 2) return '🌐';
                                  const A = 0x1F1E6;
                                  return String.fromCodePoint(A + cc.charCodeAt(0) - 65, A + cc.charCodeAt(1) - 65);
                                }

                                // Server flag (use server culture if available, e.g. en-US -> US)
                                const serverCountry = serverMeta?.culture && serverMeta.culture.includes('-') ? serverMeta.culture.split('-')[1] : undefined;
                                const serverFlag = serverCountry ? countryCodeToEmoji(serverCountry) : '🐳';

                                // Local flag: prefer deriving from the browser timezone (more reliable than locale),
                                // fallback to navigator.language when timezone mapping not available.
                                const localLocale = typeof navigator !== 'undefined' ? (navigator.language || 'en-US') : 'en-US';
                                const localTz = typeof Intl !== 'undefined' && Intl.DateTimeFormat ? Intl.DateTimeFormat().resolvedOptions().timeZone : undefined;

                                function tzToCountry(tz?: string) {
                                  if (!tz) return undefined;
                                  const map: Record<string, string> = {
                                    'Europe/London':'GB','Europe/Paris':'FR','Europe/Berlin':'DE','Europe/Madrid':'ES','Europe/Rome':'IT','Europe/Amsterdam':'NL','Europe/Brussels':'BE','Europe/Zurich':'CH','Europe/Stockholm':'SE','Europe/Helsinki':'FI','Europe/Warsaw':'PL','Europe/Vienna':'AT','Europe/Dublin':'IE','Europe/Prague':'CZ','Europe/Budapest':'HU','Europe/Athens':'GR','Europe/Lisbon':'PT','Europe/Moscow':'RU',
                                    'America/New_York':'US','America/Detroit':'US','America/Chicago':'US','America/Denver':'US','America/Los_Angeles':'US','America/Anchorage':'US','America/Phoenix':'US','America/Sao_Paulo':'BR','America/Argentina/Buenos_Aires':'AR','America/Santiago':'CL','America/Lima':'PE','America/Bogota':'CO','America/Mexico_City':'MX','America/Toronto':'CA','America/Vancouver':'CA','America/Caracas':'VE','America/Asuncion':'PY','America/Montevideo':'UY',
                                    'Asia/Tokyo':'JP','Asia/Seoul':'KR','Asia/Shanghai':'CN','Asia/Hong_Kong':'HK','Asia/Bangkok':'TH','Asia/Singapore':'SG','Asia/Kuala_Lumpur':'MY','Asia/Jakarta':'ID','Asia/Kolkata':'IN','Asia/Dubai':'AE','Asia/Jerusalem':'IL','Asia/Manila':'PH',
                                    'Australia/Sydney':'AU','Australia/Melbourne':'AU','Pacific/Auckland':'NZ',
                                    'Africa/Johannesburg':'ZA','Africa/Cairo':'EG','Africa/Lagos':'NG','Africa/Nairobi':'KE'
                                  };
                                  if (map[tz]) return map[tz];
                                  // fallback: try the last part of the timezone (city) against a small city map
                                  const parts = tz.split('/');
                                  const last = parts[parts.length - 1];
                                  const cityMap: Record<string, string> = {
                                    'Madrid':'ES','Paris':'FR','Berlin':'DE','Rome':'IT','Amsterdam':'NL','Brussels':'BE','Zurich':'CH','Stockholm':'SE','Helsinki':'FI','Warsaw':'PL','Vienna':'AT','Dublin':'IE','Prague':'CZ','Budapest':'HU','Athens':'GR','Lisbon':'PT','Moscow':'RU',
                                    'New_York':'US','Chicago':'US','Denver':'US','Los_Angeles':'US','Anchorage':'US','Phoenix':'US','Sao_Paulo':'BR','Buenos_Aires':'AR','Santiago':'CL','Lima':'PE','Bogota':'CO','Mexico_City':'MX','Toronto':'CA','Vancouver':'CA','Caracas':'VE','Asuncion':'PY','Montevideo':'UY',
                                    'Tokyo':'JP','Seoul':'KR','Shanghai':'CN','Hong_Kong':'HK','Bangkok':'TH','Singapore':'SG','Kuala_Lumpur':'MY','Jakarta':'ID','Kolkata':'IN','Dubai':'AE','Jerusalem':'IL','Manila':'PH',
                                    'Sydney':'AU','Melbourne':'AU','Auckland':'NZ','Johannesburg':'ZA','Cairo':'EG','Lagos':'NG','Nairobi':'KE'
                                  };
                                  if (cityMap[last]) return cityMap[last];
                                  const lastUnderscore = last.replace('_', '');
                                  if (cityMap[lastUnderscore]) return cityMap[lastUnderscore];
                                  return undefined;
                                }

                                const tzCountry = tzToCountry(localTz);
                                const localCountry = tzCountry ?? (localLocale.includes('-') ? localLocale.split('-')[1] : undefined);
                                const localFlag = countryCodeToEmoji(localCountry);

                                // Helper: parse offset like '+01:00' or '-05:00' into milliseconds
                                function parseOffsetToMs(ofs?: string) {
                                  if (!ofs) return 0;
                                  const m = ofs.match(/(-?)(\d{1,2}):?(\d{2})/);
                                  if (!m) return 0;
                                  const sign = m[1] === '-' ? -1 : 1;
                                  const hours = Number(m[2]);
                                  const mins = Number(m[3]);
                                  return sign * ((hours * 60 + mins) * 60 * 1000);
                                }

                                // Try to format using the server timezone (works when it's an IANA zone like 'Europe/London')
                                function tryFormatWithTZ(isoStr: string | undefined | null, tz?: string) {
                                  if (!isoStr || !tz) return null;
                                  try {
                                    return new Date(isoStr).toLocaleString(undefined, {
                                      year: 'numeric', month: '2-digit', day: '2-digit',
                                      hour: '2-digit', minute: '2-digit', second: '2-digit',
                                      timeZone: tz
                                    });
                                  } catch {
                                    return null;
                                  }
                                }

                                const ts = iso ? Date.parse(iso) : NaN;
                                const serverOffsetMs = parseOffsetToMs(serverMeta?.offset);

                                // Server time: prefer proper timezone formatting when server provides an IANA tz
                                let serverTime = '—';
                                if (!isNaN(ts)) {
                                  if (serverMeta?.timezone && serverMeta.timezone.includes('/')) {
                                    const f = tryFormatWithTZ(iso, serverMeta.timezone);
                                    serverTime = f ? `${f} ${serverMeta.offset ?? ''}` : '—';
                                  } else {
                                    // fallback: compute server-local wall time by applying offset to UTC timestamp,
                                    // then format using UTC getters so the string reflects server-local wall time
                                    const d = new Date(ts + serverOffsetMs);
                                    const pad = (n: number) => n.toString().padStart(2, '0');
                                    const y = d.getUTCFullYear();
                                    const mo = pad(d.getUTCMonth() + 1);
                                    const day = pad(d.getUTCDate());
                                    const hh = pad(d.getUTCHours());
                                    const mm = pad(d.getUTCMinutes());
                                    const ss = pad(d.getUTCSeconds());
                                    serverTime = `${mo}/${day}/${y}, ${hh}:${mm}:${ss} ${serverMeta?.offset ?? ''}`;
                                  }
                                }

                                // Local time using browser timezone
                                const localTime = iso ? new Date(iso).toLocaleString(undefined, {
                                  year: 'numeric', month: '2-digit', day: '2-digit',
                                  hour: '2-digit', minute: '2-digit', second: '2-digit', timeZoneName: 'short'
                                }) : '—';

                                const localTzName = typeof Intl !== 'undefined' && Intl.DateTimeFormat ? Intl.DateTimeFormat().resolvedOptions().timeZone : undefined;

                                // Compute difference between client and server offsets
                                const clientOffsetMs = -new Date().getTimezoneOffset() * 60000;
                                const diffMs = clientOffsetMs - serverOffsetMs;
                                const diffSign = diffMs >= 0 ? '+' : '-';
                                const diffAbsMinutes = Math.abs(diffMs) / 60000;
                                const diffH = Math.floor(diffAbsMinutes / 60);
                                const diffM = Math.floor(diffAbsMinutes % 60);
                                const diffLabel = `${diffSign}${diffH.toString().padStart(2, '0')}:${diffM.toString().padStart(2, '0')}`;

                                return (
                                  <div className="flex items-center gap-3">
                                    <div className="flex items-center gap-1" title={`Server timezone: ${serverMeta?.timezone ?? 'unknown'} ${serverMeta?.offset ?? ''}`}>
                                      <span className="text-sm">{serverFlag}</span>
                                      <span className="text-xs text-gray-500">{serverTime}</span>
                                    </div>
                                    <div className="text-xs text-gray-400">·</div>
                                    <div className="flex items-center gap-1" title={`Local timezone: ${localTzName ?? localLocale}`}>
                                      <span className="text-sm">{localFlag}</span>
                                      <span className="text-xs text-gray-500">{localTime}</span>
                                    </div>
                                    <div className="text-xs text-gray-400">({diffLabel} diff)</div>
                                  </div>
                                );
                              })()}
                            </div>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <button onClick={() => { copyText(pretty(entry.newValues)); }} className="text-xs px-2 py-1 rounded-md border">Copy new</button>
                          <button onClick={() => { copyText(pretty(entry.oldValues)); }} className="text-xs px-2 py-1 rounded-md border">Copy old</button>
                          <button onClick={() => toggle(entry.id)} className="text-xs px-2 py-1 rounded-md bg-gray-50 dark:bg-gray-800">{expanded === entry.id ? 'Collapse' : 'Details'}</button>
                        </div>
                      </div>

                      {expanded === entry.id && (
                        <div className="mt-4 grid grid-cols-1 md:grid-cols-2 gap-4">
                          <div className="p-3 rounded-lg bg-red-50 dark:bg-red-900/10 border border-red-100 dark:border-red-800 overflow-auto max-h-64">
                            <div className="text-xs font-semibold text-red-700 dark:text-red-300 mb-2">Old values</div>
                            <pre className="whitespace-pre-wrap text-xs font-mono text-gray-800 dark:text-gray-100">{entry.oldValues ? pretty(entry.oldValues) : '—'}</pre>
                          </div>
                          <div className="p-3 rounded-lg bg-emerald-50 dark:bg-emerald-900/10 border border-emerald-100 dark:border-emerald-800 overflow-auto max-h-64">
                            <div className="text-xs font-semibold text-emerald-700 dark:text-emerald-300 mb-2">New values</div>
                            <pre className="whitespace-pre-wrap text-xs font-mono text-gray-800 dark:text-gray-100">{entry.newValues ? pretty(entry.newValues) : '—'}</pre>
                          </div>
                        </div>
                      )}
                    </div>
                  ))}
                </div>

                <div className="mt-4 flex items-center justify-between">
                  <div className="flex items-center gap-2 text-sm text-gray-500">
                    <div className="text-xs">Page size:</div>
                    <select value={pageSize} onChange={e => setPageSize(Number(e.target.value))} className="rounded-md p-1 border bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 border-gray-200 dark:border-gray-700 text-sm">
                      <option value={5}>5</option>
                      <option value={10}>10</option>
                      <option value={25}>25</option>
                      <option value={50}>50</option>
                    </select>
                  </div>

                  <div className="flex items-center gap-2">
                    <button onClick={() => setPage(1)} disabled={page === 1} className="px-2 py-1 rounded-md border text-sm">« First</button>
                    <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1} className="px-2 py-1 rounded-md border text-sm">Prev</button>
                    <div className="text-sm text-gray-500">Page {page} / {totalPages}</div>
                    <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="px-2 py-1 rounded-md border text-sm">Next</button>
                    <button onClick={() => setPage(totalPages)} disabled={page === totalPages} className="px-2 py-1 rounded-md border text-sm">Last »</button>
                  </div>
                </div>
              </>
            )}
          </main>
        </div>
        </div>
      </div>
    </div>
  );
}
