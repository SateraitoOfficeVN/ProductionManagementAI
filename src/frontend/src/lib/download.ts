// 002_DD-CSV module 4 / 002_DD-SPD-CSV P-19. Hands a Blob to the browser as a download through a temporary object URL;
// no library. The anchor is never attached to the visible layout and never takes focus.
export function saveFile(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.tabIndex = -1
  anchor.click()
  // Revoked on the next task, after the browser has taken the file.
  setTimeout(() => URL.revokeObjectURL(url), 0)
}
