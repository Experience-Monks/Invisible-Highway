namespace BLE
{
void init();
void update(void (*processCommand)(const char[]));
void write(const char[]);
}