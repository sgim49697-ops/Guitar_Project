#!/usr/bin/env bash
set -euo pipefail

# sync_unity_from_windows.sh
# Windows Unity source -> Linux UnityApp mirror sync (one-way, no delete by default).

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

SRC_ROOT="${UNITY_WIN_SRC:-/mnt/c/Projects/Project_Guitar/GuitarPracticeUnity}"
DST_ROOT="${REPO_ROOT}/UnityApp"

DRY_RUN=0

if [[ "${1:-}" == "--dry-run" ]]; then
  DRY_RUN=1
  shift
fi

if [[ $# -ne 0 ]]; then
  echo "[sync] unknown argument(s): $*"
  echo "usage: $0 [--dry-run]"
  exit 1
fi

if ! command -v rsync >/dev/null 2>&1; then
  echo "[sync] rsync is required but not found."
  exit 1
fi

if [[ ! -d "${SRC_ROOT}" ]]; then
  echo "[sync] source path does not exist: ${SRC_ROOT}"
  exit 1
fi

if [[ ! -d "${DST_ROOT}" ]]; then
  echo "[sync] destination UnityApp path does not exist: ${DST_ROOT}"
  exit 1
fi

SRC_REAL="$(realpath "${SRC_ROOT}")"
DST_REAL="$(realpath "${DST_ROOT}")"

if [[ "${SRC_REAL}" == "${DST_REAL}" ]]; then
  echo "[sync] source and destination are the same path. aborting."
  exit 1
fi

MODE="apply"
if [[ ${DRY_RUN} -eq 1 ]]; then
  MODE="dry-run"
fi

echo "[sync] mode=${MODE}"
echo "[sync] source=${SRC_ROOT}"
echo "[sync] destination=${DST_ROOT}"
echo "[sync] policy=one-way windows->linux mirror, add/update only (no delete)"

TMP_LOG="$(mktemp)"
trap 'rm -f "${TMP_LOG}"' EXIT

DIR_ITEMS=(
  "Assets/Scripts"
  "Assets/Data"
  "Assets/Prefabs"
  "Assets/Plugins/WebGL"
  "Assets/Scenes"
)

FILE_ITEMS=(
  "Assets/MainScene.unity"
  "Assets/MainScene.unity.meta"
  "Packages/manifest.json"
  "Packages/packages-lock.json"
  "ProjectSettings/ProjectVersion.txt"
  "ProjectSettings/McpUnitySettings.json"
)

sync_dir() {
  local rel="$1"
  local src="${SRC_ROOT}/${rel}"
  local dst="${DST_ROOT}/${rel}"

  if [[ ! -d "${src}" ]]; then
    echo "[sync] missing source directory: ${src}"
    exit 1
  fi

  mkdir -p "${dst}"

  local cmd=(rsync -a -i)
  if [[ ${DRY_RUN} -eq 1 ]]; then
    cmd+=(-n)
  fi
  cmd+=("${src}/" "${dst}/")

  echo "[sync] + ${cmd[*]}"
  "${cmd[@]}" | tee -a "${TMP_LOG}"
}

sync_file() {
  local rel="$1"
  local src="${SRC_ROOT}/${rel}"
  local dst="${DST_ROOT}/${rel}"

  if [[ ! -f "${src}" ]]; then
    echo "[sync] missing source file: ${src}"
    exit 1
  fi

  mkdir -p "$(dirname "${dst}")"

  local cmd=(rsync -a -i)
  if [[ ${DRY_RUN} -eq 1 ]]; then
    cmd+=(-n)
  fi
  cmd+=("${src}" "${dst}")

  echo "[sync] + ${cmd[*]}"
  "${cmd[@]}" | tee -a "${TMP_LOG}"
}

for item in "${DIR_ITEMS[@]}"; do
  sync_dir "${item}"
done

for item in "${FILE_ITEMS[@]}"; do
  sync_file "${item}"
done

total_changes="$(grep -c '.' "${TMP_LOG}" || true)"
file_changes="$(grep -c '^>f' "${TMP_LOG}" || true)"
new_files="$(grep -c '^>f+++++++++' "${TMP_LOG}" || true)"
updated_files=$((file_changes - new_files))
created_dirs="$(grep -c '^cd' "${TMP_LOG}" || true)"

echo
echo "[sync] summary"
echo "[sync] changed_entries=${total_changes}"
echo "[sync] changed_files=${file_changes}"
echo "[sync] new_files=${new_files}"
echo "[sync] updated_files=${updated_files}"
echo "[sync] created_dirs=${created_dirs}"
echo "[sync] completed mode=${MODE}"
