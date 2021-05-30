# Plex Auto Intro Skip

Plex has an great feature that allows users to [Skip TV Show Intros](https://support.plex.tv/articles/skip-content/), but it requires users to click on the "Skip Intro" button each time. This can become a useless feature as soon as you are doing anything else, because you have to stop what you are doing to click on the button. Plex Auto Intro Skip is here to solve this problem.

**Note:** While [Tampermonkey](https://www.tampermonkey.net/) exists along with this [Plex auto-skip intro](https://greasyfork.org/en/scripts/404696-plex-auto-skip-intro) script, it only works if the window and tab is focused which means it is useless as soon as you are doing anything else. This is because of how the [Page Lifecycle API](https://wicg.github.io/page-lifecycle/) functions.

## Prerequisites

* Microsoft Edge (Chromium version) - Comes with latest Windows Updates.

## How to Install

1. Download latest appropriate _Stable Channel_ release of [Microsoft Edge Driver](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/).
1. Extract `msedgedriver.exe` from the downloaded ZIP archive and place it into your environment's PATH.
    * If you are not sure where to place it, place it in the `C:\Windows\System32`.
1. Download latest [Plex Auto Intro Skip Release](https://github.com/OpenNOX/PlexAutoIntroSkip/releases) and place it in a location it will be run from.
1. Right-click on `PlexAutoIntroSkip.exe`, select "Create shortcut", right-click on newly created shortcut, and select "Properties".
1. In the shortcut's properties window navigate to the end of "Target" textbox and add a space followed by "`{URL_TO_PLEX_SERVER}`" with double-quotes.
    * Finished example: `C:\bins\PlexAutoIntroSkip\PlexAutoIntroSkip.exe "https://plex.server/"`
1. Click the "Ok" button, and move (or pin) the shortcut to the desired location to launch from.

## Notes

* On initial launch you will be asked to sign in to Plex, this is because the program is using a different user data directory than default for the instance of Microsoft Edge.

## Credits

* Plex icon created by [Terence Eden](https://icon-icons.com/users/pJZ9iIZ9JPkneTWDkNfd2/icon-sets/).
