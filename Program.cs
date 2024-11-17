using System;
using System.Collections.Generic;
using System.Media;
using Tao.Sdl;

namespace MyGame
{
    
    class Program
    {
        static Image imageMenu;
        static Image imagePartida;
        static Image bloqueSerpiente;
        static Image bloqueFruta;
        static Image imageVictoria;
        static Image imageDerrota;
        static Font fuente;
        static SoundPlayer soundPlayer;

        static int selectedOption = 1; // Variable para almacenar la opción seleccionada (1-Fácil, 2-Normal, 3-Difícil)
        static int gameState = 0;       // Estado del juego (0: menú, 1: fácil, 2: normal, 3: difícil)
        static int dificultadDelay = 150; //Estado de dificultad

        static int ventanaAncho = 800, ventanaAlto = 600;
        static int bloqueTam = 20;
        static int longitudVictoria = 10; // Longitud de la serpiente para terminar
        static bool juegoCorriendo = true;
        static bool juegoGanado = false;
        static bool juegoPerdido = false;
        static Random random = new Random();

        // Direccion de la serpiente
        static int direccionX = 1, direccionY = 0;

        // Posiciones de la serpiente y la fruta
        static List<(int x, int y)> serpiente = new List<(int, int)>();
        static (int x, int y) fruta;

        static void Main(string[] args)
        {
            try
            {
                Engine.Initialize();
                imageMenu = Engine.LoadImage("assets/snake1.png");
                imagePartida = Engine.LoadImage("assets/snake2.png");
                bloqueSerpiente = Engine.LoadImage("assets/azul.png");
                bloqueFruta = Engine.LoadImage("assets/rojo.png");
                imageVictoria = Engine.LoadImage("assets/plantillaYouWin.png");
                imageDerrota = Engine.LoadImage("assets/plantillaGameOver.png");
                soundPlayer = new SoundPlayer("assets/level-vii.wav");
                fuente = Engine.LoadFont("assets/SuperMario256.ttf", 20);
                // Inicializar la serpiente y la fruta 
                serpiente.Add((ventanaAncho / 2, ventanaAlto / 2));
                GenerarFruta();
                soundPlayer.PlayLooping();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cargar los recursos: " + ex.Message);
                Environment.Exit(1);
            }
                 

            while (true)
            {
                
                CheckInputs();
                Update();
                Render();
                Sdl.SDL_Delay(dificultadDelay);  
            }
        }

        static void CheckInputs()
        {
            if (gameState == 0)
            {
                if (Engine.KeyPress(Engine.KEY_UP)) 
                {
                    selectedOption = selectedOption > 1 ? selectedOption - 1 : 3;
                }

                if (Engine.KeyPress(Engine.KEY_DOWN)) 
                {
                    selectedOption = selectedOption < 3 ? selectedOption + 1 : 1;
                }

                if (Engine.KeyPress(Engine.KEY_ENTER))
                {
                    // Cambiar el estado de dificultad
                    gameState = selectedOption;
                }
                if (Engine.KeyPress(Engine.KEY_ESC))
                {
                    Environment.Exit(0);
                }
            }
            else if (juegoGanado || juegoPerdido) // Si el juego termina
            {
                if (Engine.KeyPress(Engine.KEY_SPACE))
                {
                    gameState = 0; // Volver al menu
                    ResetGame();  // Reiniciar el juego
                }
                else if (Engine.KeyPress(Engine.KEY_ESC))
                {
                    Environment.Exit(0);
                }
            }
            else 
            {
                // Evitar controlar serpiente despues de ganar o perder
                if (juegoGanado || juegoPerdido) return;

                if (Engine.KeyPress(Engine.KEY_UP) && direccionY == 0)
                {
                    direccionX = 0;
                    direccionY = -1;
                }
                else if (Engine.KeyPress(Engine.KEY_DOWN) && direccionY == 0)
                {
                    direccionX = 0;
                    direccionY = 1;
                }
                else if (Engine.KeyPress(Engine.KEY_LEFT) && direccionX == 0)
                {
                    direccionX = -1;
                    direccionY = 0;
                }
                else if (Engine.KeyPress(Engine.KEY_RIGHT) && direccionX == 0)
                {
                    direccionX = 1;
                    direccionY = 0;
                }
                else if (Engine.KeyPress(Engine.KEY_ESC))
                {
                    Environment.Exit(0);
                }
            }
        }

        static void Update()
        {
            if (gameState != 0)
            { 
                // No actualices la serpiente si el juego ha terminado
                if (juegoGanado || juegoPerdido) return;

                // Nueva posicion de la cabeza
                int nuevoX = serpiente[0].x + direccionX * bloqueTam;
                int nuevoY = serpiente[0].y + direccionY * bloqueTam;

                // Verificar colisiones a los bordes
                if (nuevoX < 0 || nuevoX >= ventanaAncho || nuevoY < 0 || nuevoY >= ventanaAlto)
                {
                    Console.WriteLine("Juego terminado!");
                    juegoCorriendo = false;
                    juegoPerdido = true;
                    return;
                }

                // Verificar colisiones con el cuerpo
                foreach (var segmento in serpiente)
                {
                    if (segmento.x == nuevoX && segmento.y == nuevoY)
                    {
                        Console.WriteLine("Juego terminado!");
                        juegoCorriendo = false;
                        juegoPerdido = true;
                        return;
                    }
                }

                // Insertar nueva posicion al principio de la lista
                serpiente.Insert(0, (nuevoX, nuevoY));

                // Verificar si la serpiente comio la fruta
                if (nuevoX == fruta.x && nuevoY == fruta.y)
                {
                    // Generar una nueva fruta
                    GenerarFruta();

                    // Verificar si se ha alcanzado la longitud de victoria
                    if (serpiente.Count >= longitudVictoria)
                    {
                        Console.WriteLine("Has ganado!");
                        juegoGanado = true;
                        juegoCorriendo = false;
                    }
                }
                else
                {
                    // Si no ha comido, eliminar el ultimo segmento
                    serpiente.RemoveAt(serpiente.Count - 1);
                }
            }
        }

        static void Render()
        {
            Engine.Clear();
            
            if (gameState == 0) //menu
            {
                Engine.Draw(imageMenu, 0, 0);
                Engine.DrawText("Snake", 100, 100, 255, 255, 255, fuente);
                Engine.DrawText("Selecciona la dificultad", 120, 140, 255, 255, 255, fuente);
                // Opciones de dificultad 
                Engine.DrawText("1-Facil", 140, 170, (byte)(selectedOption == 1 ? 255 : 100), (byte)(selectedOption == 1 ? 255 : 100), (byte)(selectedOption == 1 ? 255 : 100), fuente);
                Engine.DrawText("2-Normal", 140, 190, (byte)(selectedOption == 2 ? 255 : 100), (byte)(selectedOption == 2 ? 255 : 100), (byte)(selectedOption == 2 ? 255 : 100), fuente);
                Engine.DrawText("3-Dificil", 140, 210, (byte)(selectedOption == 3 ? 255 : 100), (byte)(selectedOption == 3 ? 255 : 100), (byte)(selectedOption == 3 ? 255 : 100), fuente);
            }
            else  
            {
                if (gameState == 1)
                {
                    Engine.Draw(imagePartida, 0, 0);
                    dificultadDelay = 150;
                }
                if (gameState == 2)
                {
                    Engine.Draw(imagePartida, 0, 0);
                    dificultadDelay = 100;
                }
                if (gameState == 3)
                {
                    Engine.Draw(imagePartida, 0, 0);
                    dificultadDelay = 50;
                }
                // Mensajes de victoria o derrota
                if (juegoGanado)
                {
                    Engine.Draw(imageVictoria, 0, 0);
                    Engine.DrawText("Ganaste!", 120, 200, 255, 255, 255, fuente);
                    Engine.DrawText("1 - Presiona SPACE para volver al menu", 120, 230, 255, 255, 255, fuente);
                    Engine.DrawText("2 - Presiona ESC para salir", 120, 250, 255, 255, 255, fuente);
                }
                else if (juegoPerdido)
                {
                    Engine.Draw(imageDerrota, 0, 0);
                    Engine.DrawText("Perdiste!", 120, 200, 255, 255, 255, fuente);
                    Engine.DrawText("1 - Presiona SPACE para volver al menu", 120, 230, 255, 255, 255, fuente);
                    Engine.DrawText("2 - Presiona ESC para salir", 120, 250, 255, 255, 255, fuente);
                }
                else
                {
                    // Dibujar la fruta
                    Engine.Draw(bloqueFruta, fruta.x, fruta.y);

                    // Dibujar la serpiente
                    foreach (var segmento in serpiente)
                    {
                        Engine.Draw(bloqueSerpiente, segmento.x, segmento.y);
                    }
                }
            }
            Engine.Show();
        }

        static void GenerarFruta()
        {
            int x = random.Next(ventanaAncho / bloqueTam) * bloqueTam;
            int y = random.Next(ventanaAlto / bloqueTam) * bloqueTam;
            fruta = (x, y);
        }

        static void ResetGame()
        {
            // Reiniciar las variables del juego
            serpiente.Clear();
            serpiente.Add((ventanaAncho / 2, ventanaAlto / 2));
            direccionX = 1;
            direccionY = 0;
            GenerarFruta();
            juegoGanado = false;
            juegoPerdido = false;
            juegoCorriendo = true;
        }
    }
}
