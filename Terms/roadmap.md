# EqSolve Roadmap

### Immediately, it just needs to build.
- This means making the Standard Terms implement the Term interface.
- This also meant making some small changes to function signatures.

## Term capabilities

- [ ] Sine, Cosine, Tangent, Arcsine, Arccosine, Arctangent
  - These should be added simultaneously.
- [ ] Hyperbolic trigonometry (probably all 6)
  - These would just be shortcuts, because they can be described as compositions of existing functionality
  - They are still nice shortcuts to have

## Numerical formats

- [ ] Complex numbers (a+bi)
  - This might be a meta-format?
  - It's probably desirable to have a complex format mirroring each existing real one.
    - [ ] A complex format that thinly wraps two doubles, for high performance
    - [ ] One that thinly wraps two BigDecimals, for high precision
    - [ ] One that thinly wraps two BigFractions, for absurd precision
  - A generic complex struct would do all of these at once
- Maybe a BigInteger format, for people who love integral calculation?
  - (integral like integers, not like antiderivatives)
