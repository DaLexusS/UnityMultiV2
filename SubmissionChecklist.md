## פיצ'רים כלליים

סצינת פתיחה / אובייקטים עקביים:
בוצע. יש אובייקטים שממשיכים בין סצינות בעזרת `DontDestroyOnLoad`.
Scripts: `NetworkManager`, `CharacterSelectionNetworkManager`, `ReadyManager`.

סצינת תפריט ראשי + מציאת חדרים:
בוצע. יש `LobbyScene` עם יצירת חדר, רשימת חדרים, סינון חדרים והצטרפות.
Scripts: `SessionListUiHandler`, `SessionInfoListUiItem`, `NetworkManager`, `SessionMetadata`.

סצינת משחק מולטיפלייר:
בוצע. יש `GameScene` עם שחקנים, תנועה, פצצות, חיים, ניקוד ותוצאות.
Scripts: `PlayerSpawner`, `PlayerMovement`, `PlayerCombat`, `PlayerHealth`, `Bomb`, `PointsCountManager`, `ResultsUI`.

## חובה - 40 נקודות

אפשרות לפתוח או להצטרף לחדר, מינימום 3 שחקנים:
בוצע. השחקן יכול לפתוח חדר או להצטרף לחדר קיים. מספר השחקנים מוגבל ל-3 עד 4.
Scripts: `SessionListUiHandler`, `NetworkManager`, `SessionCreateRequest`.

שימוש ב-3 RPC שונים, ובכל אחד משתנה מסוג אחר:
בוצע. יש RPC עם `string`, RPC עם `int`, ו-RPC עם `PlayerRef`.
Scripts: `CharacterSelectionNetworkManager`, `ReadyManager`, `PlayerHealth`, `PointsCountManager`.

Serialize + Deserialize של Json עם לפחות 3 משתנים והעברתו ב-RPC:
בוצע. נשלח JSON של הגדרות המשחק דרך RPC.
Scripts: `MatchSettingsJson`, `MatchSettingsSync`, `NetworkManager`.

שימוש ב-NetworkTransform:
בוצע. יש `NetworkTransform` על השחקן ועל אובייקטים רשתיים.
Prefabs: `Player`, `Bomb`, `OldModel` characters.

שליטה בדמות בעזרת New Input System:
בוצע. התנועה והירייה משתמשים ב-New Input System.
Scripts: `PlayerMovement`, `PlayerCombat`.

הכנת הקליינט להפוך למאסטר בכל רגע:
בוצע. מידע חשוב נשמר ב-`[Networked]`, והמאסטר החדש מרענן את מצב הלובי וה-UI.
Scripts: `NetworkManager`, `CharacterSelectionNetworkManager`, `ReadyManager`, `PointsCountManager`.

מעבר סצינות דרך המאסטר ודרך Photon:
בוצע. רק המאסטר טוען סצינות בעזרת `Runner.LoadScene`.
Scripts: `NetworkManager`, `ReadyManager`.

הפרדה בין מאסטר קליינט לקליינט ב-UI:
בוצע. רק המאסטר רואה Start, Close ו-Kick. שחקן רגיל רואה Leave.
Scripts: `SessionListUiHandler`, `PlayerDataUi`, `NetworkManager`, `CharacterSelectionNetworkManager`.

בחירת דמות / סקין בלי כפילות:
בוצע. המאסטר נועל סקינים ומונע כפילות.
Scripts: `ReadyManager`, `PlayerItem`, `SelectorUI`, `PlayerSkinChanger`, `CharacterSelectionNetworkManager`.

רק המאסטר מחליט מספרים רנדומליים והחלטות משחק:
בוצע. המאסטר בוחר נקודת Spawn רנדומלית, בוחר שחקן רנדומלי לקבל בונוס פתיחה, ובוחר סכום בונוס רנדומלי.
Scripts: `PlayerSpawner`, `PointsCountManager`.

תנאי סיום משחק ותפריט תוצאות:
בוצע. כאשר נשאר שחקן אחרון, מוצג מסך תוצאות.
Scripts: `PointsCountManager`, `ResultsUI`, `PlayerHealth`.

מערכת ניקוד שמסתנכרנת בין השחקנים:
בוצע. הניקוד נשמר ב-`NetworkDictionary`.
Scripts: `PointsCountManager`, `ResultsUI`, `Bomb`, `PlayerHealth`.

בסיום המשחק לסגור את החדר ולהחזיר לתפריט:
בוצע. אחרי תוצאות המשחק המאסטר סוגר את החדר ומחזיר את כולם לתפריט.
Scripts: `PointsCountManager`, `NetworkManager`, `CharacterSelectionNetworkManager`, `PlayerSpawner`.

שימוש ב-NetworkRunner.Spawn:
בוצע. משתמשים ב-`Runner.Spawn` ו-`SpawnAsync`.
Scripts: `NetworkManager`, `PlayerSpawner`, `PlayerCombat`, `StartMultiGameForTest`.

שימוש ב-NetworkRunner.DeSpawn:
בוצע. משתמשים ב-`Runner.Despawn` למחיקת שחקן שמת ופצצות.
Scripts: `PlayerHealth`, `Bomb`, `CharacterObjectSpawner` אם קיים בפרויקט.

## בחירה

הגדרות חדר בפתיחת חדר רק למאסטר:
בוצע. המאסטר יכול לבחור שם חדר, מצב משחק, מפה, אזור, סיסמה ומספר שחקנים.
Scripts: `SessionListUiHandler`, `NetworkManager`, `SessionMetadata`, `SessionCreateRequest`.

NetworkMecanimAnimator:
לא בוצע.
Scripts: אין.

NetworkRigidbody 3D:
בוצע. הפצצה משתמשת ב-`NetworkRigidbody3D`.
Scripts: `Bomb`.
Prefab: `Bomb`.

שימוש ב-5 משתנים `[Networked]` לפחות:
בוצע. יש הרבה משתנים `[Networked]` בניהול סקינים, ניקוד, חיים ותוצאות.
Scripts: `ReadyManager`, `CharacterSelectionNetworkManager`, `PlayerHealth`, `PointsCountManager`, `PlayerSkinChanger`.

הצגת רשימת חדרים פתוחים וכמה שחקנים יש:
בוצע. רשימת החדרים מציגה שם חדר וכמות שחקנים.
Scripts: `SessionListUiHandler`, `SessionInfoListUiItem`, `NetworkManager`.

טיפול בשגיאות בהצטרפות לחדר:
בוצע. שגיאות מוצגות דרך מערכת הודעות שגיאה.
Scripts: `ErrorHandlerUi`, `ErrorMessageUi`, `NetworkManager`, `SessionListUiHandler`.

טיפול בניתוק שחקן מקומי ומרוחק:
בוצע חלקית. שחקן מרוחק נמחק מהרשימות. שחקן מקומי מקבל הודעת שגיאה.
Scripts: `NetworkManager`, `CharacterSelectionNetworkManager`, `ReadyManager`, `ErrorHandlerUi`.

התחברות מחדש אחרי קריסה:
לא בוצע.
Scripts: אין.

המאסטר מכריז מתי אפשר להתחיל:
בוצע. המשחק מתחיל רק אחרי שכל השחקנים מוכנים ובחרו סקין.
Scripts: `ReadyManager`, `PlayerItem`, `SelectorUI`.

ולידציה ואתחול מול המאסטר כששחקן מצטרף:
בוצע. שחקן שולח ניקניים למאסטר, והמאסטר מוסיף אותו לרשימת השחקנים.
Scripts: `NetworkManager`, `CharacterSelectionNetworkManager`, `ReadyManager`.

שחקן מחשב מחליף שחקן שהתנתק:
לא בוצע.
Scripts: אין.

הצטרפות לחדר רנדומלי לפי 3 הגדרות:
בוצע. הצטרפות רנדומלית משתמשת ב-3 הגדרות: מצב משחק, מפה ואזור.
Scripts: `SessionListUiHandler`, `SessionMetadata`, `NetworkManager`.

MatchMaking מתמשך ודינמי:
לא בוצע.
Scripts: אין.

MasterClientObject:
בוצע חלקית. `PlayerSpawner` מתנהג כאובייקט של המאסטר ומבצע החלטות מאסטר.
Scripts: `PlayerSpawner`, `NetworkManager`.

תפתיעו אותי:
Ok will try.

Host Mode:
לא בוצע. הפרויקט בנוי ב-Shared Mode.
Scripts: אין.

Host migration ב-Host Mode:
לא רלוונטי כי הפרויקט לא Host Mode.
Scripts: אין.

Dedicated Server:
לא בוצע.
Scripts: אין.

Database:
לא בוצע.
Scripts: אין.



