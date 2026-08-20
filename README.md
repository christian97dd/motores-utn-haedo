# Motores de Desarrollo I

Proyecto grupal de la materia. Unity **6000.3.5f2**: abrilo siempre con esa versión, si lo abrís con otra Unity reescribe medio proyecto y se llena de conflictos.

Las tareas las repartimos en [este tablero de Trello](https://trello.com/b/Nkd6nKZ9/motores-de-desarrollo): antes de arrancar algo, fijate ahí qué hay libre y tomalo, así no terminamos dos haciendo lo mismo.

---

## Git en cinco minutos

Git guarda **fotos** (commits) de cómo estaba el proyecto en cada momento, y GitHub es donde esas fotos viven online para que las veamos todos. Nunca se pisa el trabajo del otro: cada uno labura en su **rama** y después junta lo suyo con lo del resto.

### La primera vez

Instalá [Git](https://git-scm.com/download/win) (siguiente, siguiente, sin tocar nada) y abrí la terminal. En Windows, Git Bash. Decile quién sos, esto se hace una sola vez en la vida (o tambien podes ver la clase 2 que el profe explica como instalar GitHub Desktop):

```bash
git config --global user.name "Tu Nombre"
git config --global user.email "tumail@ejemplo.com"
```

Después traés el proyecto a tu máquina:

```bash
git clone https://github.com/christian97dd/motores-utn-haedo.git
```

Eso te crea la carpeta del proyecto. Esa carpeta es la que abrís desde Unity Hub.

### El ciclo de todos los días

Antes de empezar a laburar, traé lo último que subieron los demás:

```bash
git checkout master
git pull
```

Creá una rama para lo tuyo. El nombre es libre pero que se entienda qué estás haciendo:

```bash
git checkout -b movimiento-jugador
```

Ahora sí, abrí Unity y hacé lo tuyo. Cuando tengas algo que funcione, y no esperes a terminar todo porque conviene guardar seguido, cerrá Unity y guardá la foto:

```bash
git add .
git commit -m "El jugador se mueve con WASD"
git push -u origin movimiento-jugador
```

`add .` marca todo lo que cambiaste, `commit` saca la foto con un mensaje que explique qué hiciste, y `push` la sube a GitHub. La primera vez que subís una rama va con `-u origin nombre-de-la-rama`; después alcanza con `git push` a secas.

### Meter tu trabajo en master

Entrá al repo en GitHub y te va a aparecer un cartel para crear un **Pull Request** desde tu rama. Ponele un título, crealo y avisá al grupo. Alguien lo mira, y si está todo bien se mergea a `master`. Ahí ya podés volver a `master`, hacer `pull` y arrancar una rama nueva para lo que sigue.

### Los comandos que vas a usar siempre

```bash
git status                     # qué cambié, en qué rama estoy
git branch                     # qué ramas tengo
git checkout nombre-rama       # me paso a una rama que ya existe
git checkout -b nombre-rama    # creo una rama nueva y me paso a ella
git log --oneline              # historial de commits
git pull                       # traigo lo último de GitHub
```

Si te perdiste, `git status` te dice dónde estás parado y casi siempre te sugiere el comando que sigue. Leelo, está para eso.

## Cuando algo sale mal

Pará, no sigas tirando comandos al azar porque se empeora rápido. Copiá el error tal cual y mandalo al grupo. Peor caso, siempre podés clonar el repo de nuevo en otra carpeta y arrastrar tus archivos a mano: nada de lo que ya está en GitHub se pierde.
