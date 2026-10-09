# Modeller – Bokningstjänst

Här beskrivs de modeller som behövs i domänen: vad de innehåller, vilka regler de har och vilka testfall som verifierar reglerna.

---

## Booker (Bokare)

| Egenskap | Typ | Beskrivning |
|---|---|---|
| `Id` | `Guid` | Unik identitet |
| `Name` | `string` | Bokarens namn |
| `Email` | `string` | E-postadress som beskeden skickas till |
| `Type` | `BookerType` | `MunicipalityEmployee`, `AssociationRepresentative` eller `PrivateIndividual` |

**Regler och testfall**
- Namn får inte vara tomt.
- E-postadressen får inte vara tom och måste ha ett giltigt format.
- Bokartypen måste vara ett giltigt värde.

---

## Approver (Godkännare)

| Egenskap | Typ | Beskrivning |
|---|---|---|
| `Id` | `Guid` | Unik identitet |
| `Name` | `string` | Godkännarens namn |

**Regler och testfall**
- Namn får inte vara tomt.

---

## Premises (Lokal)

| Egenskap | Typ | Beskrivning |
|---|---|---|
| `Id` | `Guid` | Unik identitet |
| `Name` | `string` | Lokalens namn |
| `Capacity` | `int` | Maximalt antal personer |
| `BookingPolicy` | `BookingPolicy` | Policyn som gäller för lokalen |

**Regler och testfall**
- Namn får inte vara tomt.
- Kapaciteten måste vara större än 0.
- En lokal måste ha en bokningspolicy.

---

## Timeslot (Tidslucka)

Värdeobjekt.

| Egenskap | Typ | Beskrivning |
|---|---|---|
| `Start` | `DateTime` | Starttid |
| `End` | `DateTime` | Sluttid |
| `Duration` | `TimeSpan` | Beräknas som `End - Start` |

**Regler och testfall**
- Starttiden får inte vara senare än sluttiden.
- Starttiden får inte vara lika med sluttiden, eftersom tidsluckan då saknar längd.
- `Duration` ska vara `End - Start`.
- Två tidsluckor som överlappar ska upptäckas (`Overlaps`).
- Två tidsluckor som ligger direkt efter varandra (A slutar 10:00, B börjar 10:00) överlappar inte.
- Två tidsluckor med samma start- och sluttid ska vara lika (värdelikhet).

---

## BookingPolicy (Bokningspolicy)

| Egenskap | Typ | Beskrivning |
|---|---|---|
| `AllowedBookerTypes` | `IReadOnlyCollection<BookerType>` | Vilka bokartyper som får boka |
| `RequiresReview` | `bool` | Om bokningar kräver manuell granskning |
| `MinDuration` | `TimeSpan` | Kortaste tillåtna bokningslängd |
| `MaxDuration` | `TimeSpan` | Längsta tillåtna bokningslängd |
| `OpeningHours` | `TimeOnly` – `TimeOnly` | Tider då lokalen kan bokas |

**Regler och testfall**
- `MinDuration` får inte vara större än `MaxDuration`.
- Öppningstiden måste vara tidigare än stängningstiden.
- Minst en bokartyp måste vara tillåten.
- En bokare vars typ inte är tillåten nekas.
- En tidslucka som är kortare än `MinDuration` nekas.
- En tidslucka som är längre än `MaxDuration` nekas.
- En tidslucka utanför öppettiderna nekas.
- En tidslucka i det förflutna nekas.
- Policyn anger om bokningen kräver granskning.

---

## Reservation (Bokning)

Aggregatrot.

| Egenskap | Typ | Beskrivning |
|---|---|---|
| `Id` | `Guid` | Unik identitet |
| `Booker` | `Booker` | Den som bokar |
| `Premises` | `Premises` | Lokalen som bokas |
| `Timeslot` | `Timeslot` | Tidsluckan som bokas |
| `Status` | `ReservationStatus` | `Created`, `PendingReview`, `Approved` eller `Rejected` |
| `ReviewedBy` | `Approver?` | Godkännaren, om bokningen har granskats |
| `RejectionReason` | `string?` | Anledning till avslag |

**Statusflöde**

```
Created ──(policyn kräver inte granskning)──► Approved
Created ──(policyn kräver granskning)──► PendingReview ──► Approved | Rejected
```

**Regler och testfall**
- En ny bokning har statusen `Created`.
- Bokare, lokal och tidslucka är obligatoriska.
- Bokningen går till `Approved` om policyn inte kräver granskning.
- Bokningen går till `PendingReview` om policyn kräver granskning.
- Endast en bokning med statusen `PendingReview` kan godkännas eller avslås av en godkännare.
- Ett avslag kräver en anledning.
- En bokning som har statusen `Approved` eller `Rejected` kan inte ändras.
- `ReviewedBy` sätts när en godkännare godkänner eller avslår bokningen.

---

## PremisesSchedule (Lokalens bokningskalender)

Håller koll på de bokningar som finns för en lokal.

| Egenskap | Typ | Beskrivning |
|---|---|---|
| `Premises` | `Premises` | Lokalen |
| `Reservations` | `IReadOnlyCollection<Reservation>` | Aktiva bokningar |

**Regler och testfall**
- En tidslucka som överlappar en befintlig, ej avslagen bokning kan inte bokas.
- En tidslucka som överlappar en avslagen bokning kan bokas.
- Två tidsluckor som ligger direkt efter varandra kan bokas.

---

## Notification (Besked)

| Egenskap | Typ | Beskrivning |
|---|---|---|
| `Recipient` | `Booker` | Mottagaren |
| `ReservationId` | `Guid` | Bokningen som beskedet gäller |
| `Type` | `NotificationType` | `Confirmation` eller `Rejection` |
| `Message` | `string` | Meddelandetext, inklusive anledningen vid avslag |

**Regler och testfall**
- En godkänd bokning ger ett besked av typen `Confirmation`.
- En avslagen bokning ger ett besked av typen `Rejection` som innehåller anledningen.
- Inget besked skapas för en bokning som har statusen `PendingReview`.
