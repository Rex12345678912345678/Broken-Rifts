extern "C" {
    void _PrintToSystem(const char* message) {
        NSLog(@"%@", [NSString stringWithUTF8String:message]);
    }
}