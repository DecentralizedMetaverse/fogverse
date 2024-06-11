package main

type HeadData struct {
	CurrentWorld string `yaml:"currentWorld"`
	Guid         string `yaml:"guid"`
}

type WorldData struct {
	Name string   `yaml:"name"`
	GUID string   `yaml:"guid"`
	CID  []string `yaml:"cid"`
}

type WorldSingleData struct {
	CID  string  `yaml:"cid"`
	File string  `yaml:"file"`
	X    float64 `yaml:"x"`
	Y    float64 `yaml:"y"`
	Z    float64 `yaml:"z"`
	RX   float64 `yaml:"rx"`
	RY   float64 `yaml:"ry"`
	RZ   float64 `yaml:"rz"`
	SX   float64 `yaml:"sx"`
	SY   float64 `yaml:"sy"`
	SZ   float64 `yaml:"sz"`
}
