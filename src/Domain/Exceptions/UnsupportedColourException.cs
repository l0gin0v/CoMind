namespace CoMind.Domain.Exceptions;

public class UnsupportedColorException(string code) : Exception($"Colour \"{code}\" is unsupported.");
