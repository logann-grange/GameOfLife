# Jeu de la Vie


## Description du projet

Implémentation du **Jeu de la Vie** de Conway sous Unity (C#, Input System, TextMeshPro).

- Grille de taille réglable (4 à 200 cellules par côté, 64 x 48 par défaut).
- Deux modes de bord :
  - **Infinite** : la grille déborde sur une marge invisible de 2 cellules, où les cellules continuent de vivre.
  - **Symetric** : les bords opposés sont reliés.
- Vitesse de simulation réglable (ticks par seconde), mode pas à pas.
- Placement aléatoire, effacement de la grille.
- Sauvegarde et chargement de plusieurs états au format PNG.

## Mode d'emploi

### Démarrer

1. Installer Unity 6.3.
2. Cloner le dépôt, puis l'ajouter dans Unity Hub (*Add > Add project from disk*).
3. Ouvrir la scène `SampleScene` et appuyer sur **Play**.

### Commandes

| Action | Commande |
|---|---|
| Activer / désactiver une cellule | Clic gauche sur la grille |
| Lancer la simulation | Bouton **Start** ou touche `O` |
| Mettre en pause | Bouton **Pause** ou touche `P` |
| Avancer d'une génération | Bouton **Next** |
| Remplir au hasard (25 % de cellules vivantes) | Bouton **Randomize** |
| Vider la grille | Bouton **Clear** |
| Changer de mode de bord | Boutons **Infinite** / **Symetric** |
| Changer la taille | Champs **Width** / **Height** (valeurs entre 4 et 200) |
| Agrandir / réduire la grille (ratio 4:3) | Flèches haut / bas |
| Changer la vitesse | Champ **Speed** (ticks par seconde) |

### Sauvegarder et charger

1. Taper un nom dans le champ de texte (s'il est vide, un nom horodaté est utilisé), puis cliquer sur **Save**.
2. Choisir une sauvegarde dans la liste déroulante, puis cliquer sur **Load**.
3. **Delete** supprime la sauvegarde sélectionnée.

Emplacement des fichiers : dossier `saves/` à la racine du projet.


## Convention de sauvegarde PNG

Chaque sauvegarde est une image PNG sans perte, où **1 pixel = 1 cellule** de la zone visible.

| Couleur du pixel | État de la cellule |
|---|---|
| Blanc (255, 255, 255) | Vivante |
| Noir (0, 0, 0) | Morte |


| Taille de l'image | Taille de la grille |
|---|---|
| Largeur en pixels | Nombre de colonnes (Width) |
| Hauteur en pixels | Nombre de lignes (Height) |

- L'origine est en **bas à gauche**, comme dans la grille (l'axe y va vers le haut). Aucune inversion n'est nécessaire.
- Au chargement, la grille est redimensionnée à la taille de l'image (bornée entre 4 et 200).
- Seule la zone visible est sauvegardée : les cellules de la marge invisible sont toujours mortes après un chargement.

## Exemples

Génération aléatoire :
<img width="64" height="48" alt="Random" src="https://github.com/user-attachments/assets/ada7aca2-180a-44a1-bdc2-b2da537d8d50" />

Glider :
<img width="64" height="48" alt="Glider" src="https://github.com/user-attachments/assets/8de0bbe3-6682-4912-a5be-371648c51028" />

