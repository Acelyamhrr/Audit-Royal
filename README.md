# Audit Royal
**Groupe : MET25-C-A**  

Participants :  
- Fouilleul Elora
- Muharremoglu Açelya
- Zou Valentin
- Hellich Alexis

## Présentation du projet

Ce projet a pour but de réaliser un **jeu sérieux** sur le métier **d'auditeur dans une université** afin d'aider des débutants dans ce domaine à leur donner **un avant-goût de la réalisation de leur premier audit**. Pour découvrir le métier d'auditeur, nous avons réalisé un [wiki](Wiki.md). Si cela vous paraît peu clair, vous comprendrez mieux après quelques minutes de jeu.  

## Objectifs pédagogiques

Pour ce projet, nous avons choisi de nous concentrer sur les **objectifs pédagogiques suivants** :
- Comprendre les **étapes du processus d’audit** (préparation, réalisation, communication).
- Représenter la **difficulté de la collecte d’informations**, notamment à travers les échanges humains et leurs caractères.

Ainsi **Audit Royal a pour but** de :
- Fournir une expérience interactive qui **illustre la réalité du métier** d’auditeur.
- Mettre en avant la **gestion du temps, des rapports humains et de l'importance des informations recueillies**.
- Proposer un jeu permettant de **tester différents scénarios** d’audit.


## Objectifs pédagogiques détaillés
Voici l'explication approfondie des trois objectifs centraux du jeu :

### Compréhension du processus de réalisation de l'audit
Le joueur doit comprendre l’enchaînement des différentes étapes d’un audit. Ces informations sont disponibles dans le [wiki](Wiki.md), et il est essentiel de lui faire percevoir l'objectif de chaque phase du processus.  
Selon le wiki, l’audit se déroule en trois grandes phases : **préparation**, **réalisation**, puis **communication**. Pour ce projet, nous nous sommes concentrés sur les étapes de réalisation et de communication.  

Le joueur aura l'occasion d'**expérimenter la phase de récolte d'informations** (décrite dans l'objectif pédagogique suivant) ainsi que la **rédaction d'un rapport** à partir des informations qu'il aura recueillies.
Après plusieurs parties, le joueur devrait avoir compris l’importance de **planifier son travail** et avoir une **vision claire du processus** de réalisation.  

### Compréhension de la difficulté de la collecte d'informations
Cet objectif vise à faire prendre conscience au joueur des **difficultés rencontrées lors de la collecte d’informations**, surtout lorsqu’il doit interagir avec des employés. Contrairement aux autres étapes de l’audit, celle-ci repose sur des échanges humains : le joueur devra faire face à des situations où l’information est **difficile à obtenir, incomplète, dissimulée ou peu fiable**. Il devra également comprendre que **toutes les données récoltées ne sont pas pertinentes ou exactes**, ce qui complexifie la démarche.  

La collecte d’informations est une phase clé pour **comprendre le fonctionnement d’un processus**, repérer des anomalies et identifier des marges d’amélioration. Mais elle présente plusieurs défis humains :

**Difficultés de communication avec les employés**  
  - Méfiance, réticence à coopérer  
  - Réticence à dévoiler des informations sensibles  
  - Manque de collaboration  

**Fiabilité relative des informations**  
  - Témoignages contradictoires  
  - Biais ou imprécisions  
  - Abondance d’informations difficiles à trier  

**Pertinence variable des données**  
  - Informations inutiles ou hors sujet  
  - Sélection des bonnes sources  

Le joueur devra mener des **entretiens** et **discussions informelles**. Le jeu peut insister sur le fait que collecter des informations ne consiste pas à accumuler des données, mais à **faire preuve de discernement**.  

Une bonne collecte permet d’identifier plus facilement les **risques, anomalies ou défaillances** dans le processus audité. Cet objectif doit donc insister sur l’importance des échanges humains.  
Le joueur développera des compétences en **communication**, **analyse critique** et **prise de décision** dans un environnement complexe, voire hostile.

## Description des fonctionnalités
Afin d’atteindre ces objectifs, nous avons défini les fonctionnalités suivantes :

### Actions du joueur
Nous souhaitons que le joueur puisse :
- Avoir le choix entre **différents sujets d'audit**
- **Interagir avec plusieurs personnes** auditées, qui possèdent des **comportements différents** à l'égard de l'auditeur
- Choisir avec qui il interagit lorsqu'il le souhaite et naviguer entre les services
- Choisir quelles **informations** sont **vraies/fausses**
- **Produire un rapport** uniquement à partir des informations qu'il aura découvertes

### Logique de jeu
Dans la logique du jeu, il faudrait que :
- Les différentes **interactions produisent des comportements** différents
- Les **informations reçues ne soient pas toutes vraies**
- Une **mécanique de vérification** grâce à l’obtention d’informations contradictoires
- Le joueur ait un moyen d'être **limité en termes d'actions / de temps** pour représenter le temps qui passe, afin de représenter la contrainte temporelle d’un audit.
- Un **système de confiance** entre l'auditeur et les audités soit mis en place et que cette confiance **influe sur les réponses** de ces derniers, grâce aux réponses choisies par le joueur et les différents caractères des personnages

### Interface
L'interface doit pouvoir présenter :
- Un **système d'interaction** avec les audités
- Un **rendu final** qui indique si le joueur a fait du bon travail ou non avec passage devant le conseil d'administration
- Un moyen de **connaître l'état d'esprit des audités** (qu'il soit facilement visible ou non)
- Un **carnet** pour pouvoir noter les informations, décider de leurs véracités et les commenter
- Un **rapport d'audit antérieur** sur lequel se baser

### Scénario type

Afin de vous aider à plus facilement comprendre comment intégrer la plupart des fonctionnalités, nous vous proposons une version rédigée du déroulement du jeu.  

#### Briefing initial
Le joueur arrive dans un couloir et se voit convoquer au bureau de la direction. Le **directeur lui explique les attentes de l'audit et le sujet** de ce dernier. Le joueur a ensuite accès à la carte et peut naviguer entre les bâtiments/services.

#### Enquête sur le terrain
En entrant dans un bâtiment, le joueur a la possibilité de **parler à l'employé qu'il souhaite**. En fonction du caractère de ce dernier et des choix du joueur en réponse, l'employé donnera des **informations plus ou moins intéressantes** et **plus ou moins vraies**.  
Ces informations seront **inscrites automatiquement dans son carnet** se trouvant dans son sac à dos et pouvant être consulté à tout moment. Les informations seront classées par sujet et le joueur pourra, par l'intermédaire d'une case à cocher, **décider de leur véracité** (vraies ou fausses). Cette action étant réversible, le joueur pourra modifier sa décision s'il découvre de nouveaux indices. Il pourra également **écrire des commentaires** dans son carnet comme "l'employé n'avait pas l'air sûr de lui" ou autre. Ces commentaires n'auront aucune répercussion sur le rapport final, seules les décisions sur la véracité des informations en auront.

#### Rédaction du rapport
Lorsque le joueur le souhaite, ou si son nombre d'actions est arrivé à zéro, il peut **rédiger son rapport final** et le soumettre à la direction.  
La rédaction du rapport étant une étape importante de l'audit, une interface sera proposé au joueur afin qu'il puisse **glisser les informations reçues dans les colonnes vraies/fausses**. Il pourra évidemment s'appuyer sur ses notes et son carnet. Pour ajouter du réalisme, le joueur pourra également **rédiger une conclusion** et **donner son avis sur les améliorations à apporter**. Cependant, seules les décisions sur la véracité des informations aura une importance sur la note finale, le jeu se concentrant surtout sur les phases 2 et 3 d'un audit (voir [wiki](Wiki.md)).  
Une fois son rapport rendu, le joueur se retrouvera convoqué par **le conseil d'administration** qui **formulera un avis sur l'audit réalisé** et qui **donnera une note** à l'audit.


## Contraintes de développement
La technologie utilisée et les points abordés par le jeu sont à notre appréciation. Cependant, il faut permettre aux joueurs d'expérimenter des **scénarios différents** et que chaque partie soit différente l'une de l'autre. En effet, les choix pris ne doivent pas être linéaires au fil des parties mais résulter de **données aléatoirement sélectionnées** au début du jeu, comme le caractère des personnages et la véracité de telle ou telle information. Ainsi, les personnes ayant pour objectif de devenir auditeurs pourront ainsi comprendre la complexité du métier et l'approche à avoir en fonction du caractère de l'employé audité.



## Lien de téléchargement
*A compléter, en cours de développement*  

## Captures d'écran
*A compléter, en cours de développement*  

## Procédures d'installation et d'exécution
*A compléter, en cours de développement*