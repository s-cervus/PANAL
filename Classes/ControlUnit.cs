using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace PA
{
    public enum DbMode { Local, Server }

    public static class ControlUnit
    {
        // ========================================================
        // CONFIGURACIÓN Y ESTADOS DE SESIÓN GLOBALES
        // ========================================================
        public static DbMode CurrentMode { get; set; } = DbMode.Local; // Switch maestro

        public static string Uniq_ID { get; private set; } = "";
        public static string CurrentRole { get; private set; } = "";
        public static int IntentosFallidos { get; private set; } = 0;
        public static string CurrentUser { get; private set; } = "";

        // Directorio exclusivo de tu sistema para evitar colisiones en la USB o disco
        private const string DirectorioBunker = "data_PDC7";
        private const string LocalConnectionString = @"Data Source=data_PDC7\login.db;Version=3;Foreign Keys=True;";
        private const string ServerConnectionString = @"Server=tu_servidor_vps;Database=EmpresaDB;Integrated Security=True;";

        private const string PEPPER = "Equipo█5";
        private const int _GOLD_HEARTH = 65537; // Constante mutante para Key Stretching
        private const string LOCK_SIGNATURE = "system_final_door_lock"; // Identificador DOM

        // ========================================================
        // CRIPTO BLOB
        // ========================================================

        /// <summary>
        /// Genera una sal (Caramel) de 16 bytes crudos usando entropía criptográfica por hardware.
        /// </summary>
        private static byte[] _get_caramel()
        {
            byte[] buffer = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buffer);
            }
            return buffer;
        }

        /// <summary>
        /// Procesa la contraseña de forma paramétrica uniendo bloques Unicode nativos.
        /// Ejecuta 65,537 iteraciones de SHA256 y retorna un BLOB puro de 32 bytes.
        /// </summary>
        private static byte[] _get_data(string usuario, string password, byte[] caramel)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] userBytes = Encoding.Unicode.GetBytes(usuario);
                byte[] passBytes = Encoding.Unicode.GetBytes(password);
                byte[] pepperBytes = Encoding.Unicode.GetBytes(PEPPER);
                byte[] blockBytes = Encoding.Unicode.GetBytes("███████");

                int totalLength = blockBytes.Length * 2 + userBytes.Length + passBytes.Length + caramel.Length + pepperBytes.Length;
                byte[] srecipeBytes = new byte[totalLength];
                int offset = 0;

                Buffer.BlockCopy(blockBytes, 0, srecipeBytes, offset, blockBytes.Length); offset += blockBytes.Length;
                Buffer.BlockCopy(userBytes, 0, srecipeBytes, offset, userBytes.Length); offset += userBytes.Length;
                Buffer.BlockCopy(passBytes, 0, srecipeBytes, offset, passBytes.Length); offset += passBytes.Length;
                Buffer.BlockCopy(caramel, 0, srecipeBytes, offset, caramel.Length); offset += caramel.Length;
                Buffer.BlockCopy(pepperBytes, 0, srecipeBytes, offset, pepperBytes.Length); offset += pepperBytes.Length;
                Buffer.BlockCopy(blockBytes, 0, srecipeBytes, offset, blockBytes.Length);

                byte[] procBytes = srecipeBytes;
                for (int i = 0; i < _GOLD_HEARTH; i++)
                {
                    procBytes = sha256.ComputeHash(procBytes);
                }

                return procBytes;
            }
        }

        private static bool _compare_bytes(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i]; // XOR en tiempo constante para anular ataques de canal lateral (Timing Attacks)
            }
            return result == 0;
        }

        private static string _generate_uniq_id(string usuario)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] input = Encoding.Unicode.GetBytes(usuario.ToLower().Trim() + PEPPER);
                byte[] hash = sha256.ComputeHash(input);
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16).ToLower();
            }
        }

        /// <summary>
        /// Capa de abstracción superior: Consulta dinámicamente la tabla cID 
        /// para resolver el rol en RAM sin usar condicionales rígidos.
        /// </summary>
        private static void _establish_session(string usuario, int uid, SQLiteConnection conShared = null)
        {
            Uniq_ID = _generate_uniq_id(usuario);

            CurrentUser = usuario.Trim();

            int rolId = uid / 100000; // Extrae el prefijo del rol (Ej: 100045 -> 1)

            if (CurrentMode == DbMode.Server)
            {
                // Fallback de seguridad si consultas el srv remoto.
                if (rolId == 1) CurrentRole = "SUPER_ADMIN";
                else if (rolId == 2) CurrentRole = "ADMIN_RW";
                else CurrentRole = "NORMAL_USER";
                return;
            }

            string query = "SELECT element FROM cID WHERE ID = @id LIMIT 1";

            // Reutilizamos la conexión activa del login para no reabrir el descriptor de archivo
            bool travellingConnectionInternal = (conShared == null);
            SQLiteConnection con = travellingConnectionInternal ? new SQLiteConnection(LocalConnectionString) : conShared;

            using (SQLiteCommand cmd = new SQLiteCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@id", rolId);
                try
                {
                    if (travellingConnectionInternal) con.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // Transformamos el valor de la tabla cID a mayúsculas para control de RAM
                        CurrentRole = result.ToString().ToUpper() + (rolId == 1 ? "_ADMIN" : (rolId == 5 || rolId == 2 ? "_RW" : "_RO"));
                    }
                    else
                    {
                        CurrentRole = "UNKNOWN_ROLE_DENIED";
                    }
                }
                catch
                {
                    CurrentRole = "GUEST_RESTRICTED";
                }
                finally
                {
                    if (travellingConnectionInternal) con.Close();
                }
            }

            /* ===================================================================
             * POLÍTICAS DE DENEGACIÓN
             * ===================================================================
             * Aqui van las políitcas de denegación
             * ===================================================================
             */
        }


        // ========================================================
        // MANAGE DOM++ ZONE (DOM)
        // ========================================================

        public static bool IsLockedDown()
        {
            string query = "SELECT Lockdown FROM Login WHERE Username = @lockSig";
            using (SQLiteConnection con = new SQLiteConnection(LocalConnectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@lockSig", LOCK_SIGNATURE);
                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            long unixTimestampBaneo = Convert.ToInt64(result);
                            long unixTimeActual = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                            if (unixTimeActual < unixTimestampBaneo)
                            {
                                return true; // El escudo protector del DOM sigue activo
                            }
                            else
                            {
                                _limpiar_lockdown(con); // El castigo expiró, restauramos.
                            }
                        }
                    }
                    catch { return false; }
                }
            }
            return false;
        }

        public static void IncrementarOActivarLockdown()
        {
            long unixTimeActual = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long nuevoBloqueoUnix;

            string queryBuscar = "SELECT Lockdown FROM Login WHERE Username = @lockSig";
            using (SQLiteConnection con = new SQLiteConnection(LocalConnectionString))
            {
                try
                {
                    con.Open();
                    object actualLock = null;
                    using (SQLiteCommand cmdBuscar = new SQLiteCommand(queryBuscar, con))
                    {
                        cmdBuscar.Parameters.AddWithValue("@lockSig", LOCK_SIGNATURE);
                        actualLock = cmdBuscar.ExecuteScalar();
                    }

                    if (actualLock != null && actualLock != DBNull.Value)
                    {
                        // LÓGICA ACUMULATIVA: Si intentas golpear la puerta estando en DOM, sumamos 1 hora (3600s) extra en caliente
                        long timestampActualDb = Convert.ToInt64(actualLock);
                        long baseCalculo = Math.Max(timestampActualDb, unixTimeActual);
                        nuevoBloqueoUnix = baseCalculo + 3600;
                    }
                    else
                    {
                        // Primer baneo por fallar 3 veces consecutivas
                        nuevoBloqueoUnix = unixTimeActual + 3600;
                    }

                    string queryUpsert = @"
                        INSERT INTO Login (Username, uID, Data, Caramel, Lockdown) 
                        VALUES (@sig, 0, x'00', x'00', @lockTime)
                        ON CONFLICT(Username) DO UPDATE SET Lockdown = @lockTime;";

                    using (SQLiteCommand cmdUpsert = new SQLiteCommand(queryUpsert, con))
                    {
                        cmdUpsert.Parameters.AddWithValue("@sig", LOCK_SIGNATURE);
                        cmdUpsert.Parameters.AddWithValue("@lockTime", nuevoBloqueoUnix);
                        cmdUpsert.ExecuteNonQuery();
                    }
                }
                catch { /* Manejo sigiloso ante colisión de hilos */ }
            }
            _establish_session("nobody", 0, null);
            // Degradación instantánea de credenciales en RAM
        }

        private static void _limpiar_lockdown(SQLiteConnection con)
        {
            string query = "DELETE FROM Login WHERE Username = @lockSig";
            using (SQLiteCommand cmd = new SQLiteCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@lockSig", LOCK_SIGNATURE);
                cmd.ExecuteNonQuery();
                IntentosFallidos = 0; // Reseteamos la telemetría local de errores
            }
        }

        // ========================================================
        // SAFE REG BY ROLE (code: 1, 2, 3)
        // ========================================================

        public static int RegistrarUsuario(string username, string password)
        {
            int uid = 3;
            if (IsLockedDown()) return 3; // Operación denegada por infraestructura bajo fuego DOM

            string userClean = username.ToLower().Trim();
            byte[] caramel = _get_caramel();
            byte[] dataBytes = _get_data(userClean, password, caramel);

            if (CurrentMode == DbMode.Local)
            {
                using (SQLiteConnection con = new SQLiteConnection(LocalConnectionString))
                {
                    string query = "INSERT INTO Login (Username, uID, Data, Caramel, Lockdown) VALUES (@user, @uid, @data, @caramel, NULL)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@user", userClean);
                        cmd.Parameters.AddWithValue("@uid", uid);
                        cmd.Parameters.Add("@data", DbType.Binary).Value = dataBytes;
                        cmd.Parameters.Add("@caramel", DbType.Binary).Value = caramel;

                        try
                        {
                            con.Open();
                            int ejecutado = cmd.ExecuteNonQuery();
                            return ejecutado > 0 ? 1 : 3;
                        }
                        catch (SQLiteException ex)
                        {
                            if (ex.ResultCode == SQLiteErrorCode.Constraint) return 2; // El Username o el uID violan la restricción UNIQUE
                            return 3;
                        }
                    }
                }
            }
            else
            {
                // Réplica en espejo para el srv corporativo
                using (SqlConnection con = new SqlConnection(ServerConnectionString))
                {
                    string query = "INSERT INTO Login (Username, uID, Data, Caramel, Lockdown) VALUES (@user, @uid, @data, @caramel, NULL)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@user", userClean);
                        cmd.Parameters.AddWithValue("@uid", uid);
                        cmd.Parameters.Add("@data", SqlDbType.VarBinary).Value = dataBytes;
                        cmd.Parameters.Add("@caramel", SqlDbType.VarBinary).Value = caramel;

                        try
                        {
                            con.Open();
                            int ejecutado = cmd.ExecuteNonQuery();
                            return ejecutado > 0 ? 1 : 3;
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Number == 2627 || ex.Number == 2601) return 2; // Clave duplicada en SQL Server
                            return 3;
                        }
                    }
                }
            }
        }

        /*
        public static int AutoRegistrarEmpleado(string username, string password)
        {
            int nuevoUid = 300001; // El prefijo de rol es 3 (empleado)
            string queryMax = "SELECT MAX(uID) FROM Login WHERE uID >= 300000 AND uID < 400000";

            using (SQLiteConnection con = new SQLiteConnection(LocalConnectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(queryMax, con))
                {
                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            nuevoUid = Convert.ToInt32(result) + 1; // Generación secuencial automática
                        }
                    }
                    catch { }
                }
            }
            return RegistrarUsuario(username, password, nuevoUid);
        }
        */




        // ========================================================
        // AUTH Cryptan
        // ========================================================

        public static bool CheckANDRotate(string usuario, string password)
        {
            if (IsLockedDown())
            {
                // Lockdown in increase
                IncrementarOActivarLockdown();
                return false;
            }

            string userClean = usuario.ToLower().Trim();

            if (CurrentMode == DbMode.Local)
            {
                // Traemos una única fila específica eliminando los escaneos lineales costosos
                string query = "SELECT uID, Data, Caramel FROM Login WHERE Username = @user";
                using (SQLiteConnection con = new SQLiteConnection(LocalConnectionString))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@user", userClean);
                        try
                        {
                            con.Open();
                            using (SQLiteDataReader lector = cmd.ExecuteReader())
                            {
                                if (lector.Read())
                                {
                                    int uidDb = Convert.ToInt32(lector["uID"]);
                                    byte[] dataDb = (byte[])lector["Data"];
                                    byte[] caramelDb = (byte[])lector["Caramel"];

                                    byte[] hashIntento = _get_data(userClean, password, caramelDb);

                                    if (_compare_bytes(hashIntento, dataDb))
                                    {
                                        lector.Close();
                                        IntentosFallidos = 0; // Limpiamos el contador

                                        // Resolvemos la sesión inyectando la conexión compartida para máxima velocidad
                                        _establish_session(userClean, uidDb, con);

                                        // Destruimos el Caramel previo forzando la mutación criptográfica instantánea
                                        _mutar_caramel_local(con, userClean, password);
                                        return true;
                                    }
                                }
                            }
                        }
                        catch { return false; }
                    }
                }
            }
            else
            {
                // Autenticación remota
                string query = "SELECT uID, Data, Caramel FROM Login WHERE Username = @user";
                using (SqlConnection con = new SqlConnection(ServerConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@user", userClean);
                        try
                        {
                            con.Open();
                            using (SqlDataReader lector = cmd.ExecuteReader())
                            {
                                if (lector.Read())
                                {
                                    int uidDb = Convert.ToInt32(lector["uID"]);
                                    byte[] dataDb = (byte[])lector["Data"];
                                    byte[] caramelDb = (byte[])lector["Caramel"];

                                    byte[] hashIntento = _get_data(userClean, password, caramelDb);

                                    if (_compare_bytes(hashIntento, dataDb))
                                    {
                                        lector.Close();
                                        IntentosFallidos = 0;
                                        _establish_session(userClean, uidDb, null);
                                        _mutar_caramel_server(con, userClean, password);
                                        return true;
                                    }
                                }
                            }
                        }
                        catch { return false; }
                    }
                }
            }

            // Si la ejecución llega a este punto, las credenciales fallaron
            IntentosFallidos++;
            if (IntentosFallidos >= 3)
            {
                IncrementarOActivarLockdown(); // Detonación automática del DOM
            }
            return false;
        }

        private static void _mutar_caramel_local(SQLiteConnection con, string user, string pass)
        {
            byte[] nuevoCaramel = _get_caramel();
            byte[] nuevoData = _get_data(user, pass, nuevoCaramel);

            string query = "UPDATE Login SET Data = @nd, Caramel = @nc WHERE Username = @user";
            using (SQLiteCommand cmd = new SQLiteCommand(query, con))
            {
                cmd.Parameters.Add("@nd", DbType.Binary).Value = nuevoData;
                cmd.Parameters.Add("@nc", DbType.Binary).Value = nuevoCaramel;
                cmd.Parameters.AddWithValue("@user", user);
                cmd.ExecuteNonQuery();
            }
        }

        private static void _mutar_caramel_server(SqlConnection con, string user, string pass)
        {
            byte[] nuevoCaramel = _get_caramel();
            byte[] nuevoData = _get_data(user, pass, nuevoCaramel);

            string query = "UPDATE Login SET Data = @nd, Caramel = @nc WHERE Username = @user";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@nd", SqlDbType.VarBinary).Value = nuevoData;
                cmd.Parameters.Add("@nc", SqlDbType.VarBinary).Value = nuevoCaramel;
                cmd.Parameters.AddWithValue("@user", user);
                cmd.ExecuteNonQuery();
            }
        }

        // ========================================================
        // SECCIÓN DE INICIALIZACIÓN
        // ========================================================

        public static void InicializarEntornoLocal()
        {
            // Forzar la creación de la carpeta de datos PDC7
            if (!Directory.Exists(DirectorioBunker))
            {
                Directory.CreateDirectory(DirectorioBunker);
            }

            string rutaArchivo = Path.Combine(DirectorioBunker, "login.db");
            bool recienCreado = !File.Exists(rutaArchivo);

            using (SQLiteConnection connection = new SQLiteConnection(LocalConnectionString))
            {
                connection.Open();

                if (recienCreado)
                {
                    // Tabla Login adaptada al estándar STRICT binario puro
                    string tableLogin = @"
                        CREATE TABLE ""Login"" (
                            ""Username"" TEXT UNIQUE,
                            ""uID""      INTEGER NOT NULL UNIQUE,
                            ""Data""     BLOB NOT NULL,
                            ""Caramel""  BLOB NOT NULL,
                            ""Lockdown"" INTEGER,
                            PRIMARY KEY(""Username"")
                        ) STRICT;";

                    // Tabla cID con llave compuesta para control estricto de roles
                    string tableCID = @"
                        CREATE TABLE ""cID"" (
                            ""ID""      INTEGER,
                            ""element"" TEXT,
                            PRIMARY KEY(""ID"", ""element"")
                        );";

                    using (SQLiteCommand cmd = new SQLiteCommand(tableLogin, connection)) cmd.ExecuteNonQuery();
                    using (SQLiteCommand cmd = new SQLiteCommand(tableCID, connection)) cmd.ExecuteNonQuery();

                    // Alimentamos la abstracción de roles
                    string insertarCat = @"
                        INSERT INTO cID (ID, element) VALUES 
                        (0, 'DOM'),
                        (1, 'super'),
                        (2, 'admin'),
                        (3, 'empleado'),
                        (4, 'auditor'),
                        (5, 'ajustes'),
                        (999, 'test');";

                    using (SQLiteCommand cmdCat = new SQLiteCommand(insertarCat, connection)) cmdCat.ExecuteNonQuery();

                    // Cuentas de desarrollo iniciales empaquetadas (Formato uID: [Rango][Secuencia])
                    _registrar_init_local(connection, "super", "super7", 100001);
                    _registrar_init_local(connection, "admin_sec", "admin123", 200001);
                    _registrar_init_local(connection, "empleado", "push99", 300001);
                    _registrar_init_local(connection, "auditor", "readonly", 400001);
                    _registrar_init_local(connection, "test", "test", 99900001);
                }
            }

            // ========================================================
            // EXTRACCIÓN AUTOMÁTICA DE PLANTILLAS EXTRA DESDE RESOURCES/RAW
            // ========================================================
            // Aquí es donde el ControlUnit crea la carpeta data_PDC7
            // inyectando los archivos vacíos de las otras bases contables desde recursos internos:
            /*
            string rutaProductos = Path.Combine(DirectorioBunker, "store.db");
            if (!File.Exists(rutaProductos))
            {
                File.WriteAllBytes(rutaProductos, Properties.Resources.store_template_db);
            }
            */
        }

        private static void _registrar_init_local(SQLiteConnection con, string user, string pass, int uid)
        {
            byte[] caramel = _get_caramel();
            byte[] data = _get_data(user, pass, caramel);
            string query = "INSERT INTO Login (Username, uID, Data, Caramel, Lockdown) VALUES (@user, @uid, @data, @caramel, NULL)";
            using (SQLiteCommand cmd = new SQLiteCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@user", user);
                cmd.Parameters.AddWithValue("@uid", uid);
                cmd.Parameters.Add("@data", DbType.Binary).Value = data;
                cmd.Parameters.Add("@caramel", DbType.Binary).Value = caramel;
                cmd.ExecuteNonQuery();
            }
        }
    }
}

